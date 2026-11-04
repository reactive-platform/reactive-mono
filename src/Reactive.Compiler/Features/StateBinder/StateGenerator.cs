using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

[Generator]
internal class StateGenerator : IIncrementalGenerator {
    public const string GeneratorName = "Reactive_StateGenerator";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // Every bindable property of an initialized type in Debug, so completion can suggest them before they're used
        var generateAll = BuildConfiguration.CreateIsDebugProvider(context);

        // `new Foo { sB }` without `=` is parsed as a collection initializer, so we match both
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InitializerExpressionSyntax {
                    RawKind: (int)SyntaxKind.ObjectInitializerExpression or (int)SyntaxKind.CollectionInitializerExpression,
                    Parent: BaseObjectCreationExpressionSyntax or AssignmentExpressionSyntax
                },
                transform: static (ctx, _) => ctx
            )
            .Combine(generateAll)
            .SelectMany(static (pair, ct) =>
                GetCandidates((InitializerExpressionSyntax)pair.Left.Node, pair.Left.SemanticModel, pair.Right, ct)
            );

        // Group by containing type
        var groupedByType = candidates
            .Collect()
            .SelectMany((candidates, _) =>
                candidates
                    .Distinct()
                    .GroupBy(
                        x => x.targetProp,
                        x => x.genName,
                        SymbolEqualityComparer.Default
                    )
                    .GroupBy(
                        x => x.Key.ContainingType.OriginalDefinition,
                        SymbolEqualityComparer.Default
                    )
            );

        // Generate source code
        context.RegisterSourceOutput(
            groupedByType,
            static (spc, typeGroup) => {
                var type = (INamedTypeSymbol?)typeGroup.Key;
                if (type == null) return;

                var genericsType = type.IsExtension ? (INamedTypeSymbol)type.ExtensionParameter!.Type : type;

                var ext = GenerateTypeExtension(type, genericsType, typeGroup);
                var identifier = type.GetTypeIdentifier();

                var file = $"Reactive_{identifier}_StateExt.g.cs";
                spc.AddSource(file, ext);
            }
        );
    }

    private static IEnumerable<(ISymbol targetProp, string genName)> GetCandidates(
        InitializerExpressionSyntax initializer,
        SemanticModel semanticModel,
        bool includeUnused,
        CancellationToken cancellationToken
    ) {
        cancellationToken.ThrowIfCancellationRequested();

        if (GetPatterns(initializer, semanticModel) is not { } patterns) {
            return [];
        }

        // Get type of the object that is being initialized
        if (SyntaxExtensions.FindInitializerType(initializer, semanticModel) is not { } type) {
            return [];
        }

        return includeUnused
            ? CollectAllProperties(initializer, patterns, type, semanticModel)
            : CollectMatchingProperties(initializer, patterns, type, semanticModel);
    }
    
    private static List<(ISymbol targetProp, string genName)> CollectMatchingProperties(
        InitializerExpressionSyntax initializer,
        string[] patterns,
        ITypeSymbol type,
        SemanticModel semanticModel
    ) {
        var result = new List<(ISymbol targetProp, string genName)>();

        foreach (var expression in initializer.Expressions) {
            if (expression is not AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment) {
                continue;
            }
            
            if (!assignment.Right.IsStateExpression(semanticModel)) {
                continue;
            }

            // We only need unresolved symbols
            if (semanticModel.GetSymbolInfo(assignment.Left).Symbol != null) {
                continue;
            }

            if (MatchBinding(assignment, patterns, type, semanticModel) is { } prop) {
                result.Add((prop.OriginalDefinition, assignment.Left.ToString()));
            }
        }

        return result;
    }
    
    internal static IPropertySymbol? MatchBinding(
        AssignmentExpressionSyntax assignment,
        string[] patterns,
        ITypeSymbol type,
        SemanticModel semanticModel
    ) {
        var statePropName = assignment.Left.ToString();
        var position = assignment.Left.SpanStart;

        foreach (var pattern in patterns) {
            var rex = pattern.Replace("{}", "([A-Za-z0-9_]+)");
            if (Regex.Match(statePropName, rex) is not { Success: true } match) {
                continue;
            }

            var matchedPropName = match.Groups[1].Value;

            var symbols = semanticModel.LookupSymbols(
                position,
                type,
                matchedPropName,
                includeReducedExtensionMethods: true // CRUCIAL!! Captures extension properties
            );

            if (symbols.FirstOrDefault() is IPropertySymbol prop && IsBindable(prop, semanticModel)) {
                return prop;
            }
        }

        return null;
    }
    
    private static List<(ISymbol targetProp, string genName)> CollectAllProperties(
        InitializerExpressionSyntax initializer,
        string[] patterns,
        ITypeSymbol type,
        SemanticModel semanticModel
    ) {
        var result = new List<(ISymbol targetProp, string genName)>();
        var position = initializer.OpenBraceToken.Span.End;

        var members = semanticModel.LookupSymbols(
            position,
            type,
            includeReducedExtensionMethods: true // CRUCIAL!! Captures extension properties
        );

        foreach (var member in members) {
            if (member is not IPropertySymbol prop || !IsBindable(prop, semanticModel)) {
                continue;
            }

            foreach (var pattern in patterns) {
                result.Add((prop.OriginalDefinition, pattern.Replace("{}", prop.Name)));
            }
        }

        return result;
    }
    
    private static bool IsBindable(IPropertySymbol prop, SemanticModel semanticModel) {
        if (prop is not { IsStatic: false, IsIndexer: false, SetMethod: { IsInitOnly: false } setter }) {
            return false;
        }

        // Extensions on a bare type parameter (e.g. `extension<T>(T comp)`) can't be targeted by a typed extension.
        // Checking the original definition, as that's what gets generated and the lookup substitutes T
        var containingType = prop.OriginalDefinition.ContainingType;
        var receiver = containingType.IsExtension ? containingType.ExtensionParameter?.Type : containingType;

        if (receiver is not INamedTypeSymbol) {
            return false;
        }

        // The binding is generated into a top-level class of this assembly, so everything it mentions has to be
        // reachable from there, not just from the initializer, which might be inside a type seeing its private members
        var compilation = semanticModel.Compilation;
        var assembly = compilation.Assembly;

        if (!compilation.IsSymbolAccessibleWithin(setter, assembly) ||
            !compilation.IsSymbolAccessibleWithin(receiver, assembly) ||
            !compilation.IsSymbolAccessibleWithin(prop.Type, assembly)) {
            return false;
        }

        // Raw states are excluded from the generation process
        return prop.GetDerivedAttribute<RawStateAttribute>(semanticModel) == null;
    }

    internal static string[]? GetPatterns(SyntaxNode node, SemanticModel semanticModel) {
        if (node.GetEnclosingMember() is not { } methodSyntax) {
            return null;
        }

        if (semanticModel.GetDeclaredSymbol(methodSyntax) is not { } method) {
            return null;
        }

        if (method.GetDerivedAttribute<StateGenAttribute>(semanticModel) is not { } attr) {
            return null;
        }

        // This expression also checks if Enabled is not null (defined)
        // so in case it IS null, it won't return, defaulting to true 
        if (attr.GetNamedArgument("Enabled") is { Value: not true }) {
            return null;
        }

        string[] patterns;
        if (attr.GetNamedArgument("Patterns") is { } patternsArg) {
            patterns = patternsArg.Values.Select(x => x.Value).OfType<string>().ToArray();
        } else {
            patterns = ["s{}"];
        }

        return patterns;
    }

    private static string GenerateTypeExtension(INamedTypeSymbol containingType, INamedTypeSymbol type, IEnumerable<IGrouping<ISymbol?, string>> propGroups) {
        var typeIdentifier = type.GetTypeIdentifier();
        var genericArguments = CompilerHelper.GenerateGenericsDecl(containingType, true);
        var constraints = CompilerHelper.GenerateConstrainments(containingType).Trim();
        var import = CompilerHelper.GenerateImportOf(containingType);

        var buffer = new StringBuilder();

        buffer.AppendLine("#nullable enable");
        buffer.AppendLine();

        if (import != null) {
            buffer.AppendLine(import);
            buffer.AppendLine();
        }

        buffer.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        buffer.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorName}\", \"1.0\")]");
        buffer.AppendLine($"internal static class Reactive_{typeIdentifier}_StateGenExt {{");

        buffer.Append($"    extension{genericArguments}({type} obj)");
        if (constraints.Length > 0) {
            buffer.Append($" {constraints}");
        }
        buffer.AppendLine(" {");

        AppendProps(buffer, type, propGroups);

        buffer.AppendLine("    }");
        buffer.AppendLine("}");

        return buffer.ToString();
    }

    private static void AppendProps(StringBuilder buffer, INamedTypeSymbol type, IEnumerable<IGrouping<ISymbol?, string>> propGroups) {
        var binder = StateGeneratorUtils.GetBinderMethod(type);
        var first = true;

        foreach (var nameGroup in propGroups) {
            var prop = nameGroup.Key!;
            var propType = SemanticExtensions.GetReturnType(prop)!;

            foreach (var propName in nameGroup) {
                // Blank lines go between the props only
                if (!first) {
                    buffer.AppendLine();
                }

                first = false;
                AppendProp(buffer, propType, propName, prop.Name, binder);
            }
        }
    }

    private static void AppendProp(StringBuilder buffer, ITypeSymbol stateType, string propName, string sourcePropName, string binderMethodName) {
        const string Indent = "        ";

        var binderType = $"{StateGeneratorUtils.StateBinderPath}<{stateType}, {StateGeneratorUtils.StatePath}<{stateType}>>";

        buffer.AppendLine($"{Indent}[Reactive.Compiler.SetsRequiredAttribute(Names = [\"{sourcePropName}\"])]");
        buffer.AppendLine($"{Indent}public {binderType} {propName} {{");
        buffer.AppendLine($"{Indent}    set {{");
        buffer.AppendLine($"{Indent}        value.{binderMethodName}(obj, static (x, y) => x.{sourcePropName} = y);");
        buffer.AppendLine($"{Indent}    }}");
        buffer.AppendLine($"{Indent}}}");
    }
}