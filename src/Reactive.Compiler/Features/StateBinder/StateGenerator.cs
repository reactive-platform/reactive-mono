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
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // Filter assignment expressions and get semantic model
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression },
                transform: static (ctx, ct) => {
                    var assignment = (AssignmentExpressionSyntax)ctx.Node;
                    var semanticModel = ctx.SemanticModel;
                    return GetCandidate(assignment, semanticModel, ct);
                }
            )
            .Where(static candidate => candidate.HasValue)
            .Select(static (candidate, _) => candidate!.Value);

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

    private static (ISymbol targetProp, string genName)? GetCandidate(
        AssignmentExpressionSyntax assignment,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    ) {
        cancellationToken.ThrowIfCancellationRequested();

        if (GetPatterns(assignment, semanticModel) is not { } patterns) {
            return null;
        }

        // Ignoring state type here as we simply need to ensure that the resulting 
        // object is IState, the target type is taken from the target property
        if (!assignment.Right.IsStateExpression(semanticModel)) {
            return null;
        }

        // We only need unresolved symbols
        if (semanticModel.GetSymbolInfo(assignment.Left).Symbol != null) {
            return null;
        }

        if (GetTargetProperty(patterns, assignment, semanticModel) is not { } tuple) {
            return null;
        }

        return tuple;
    }

    private static string[]? GetPatterns(AssignmentExpressionSyntax assignment, SemanticModel semanticModel) {
        if (assignment.GetEnclosingMember() is not { } methodSyntax) {
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

    private static (ISymbol, string)? GetTargetProperty(string[] patterns, AssignmentExpressionSyntax assignment, SemanticModel semanticModel) {
        // Get type of the object that is being initialized
        var containingType = SyntaxExtensions.FindInitializerType(assignment.Parent!, semanticModel);
        if (containingType == null) {
            return null;
        }

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
                containingType,
                matchedPropName,
                includeReducedExtensionMethods: true // CRUCIAL!! Captures extension properties
            );

            var symbol = symbols.FirstOrDefault();

            if (symbol is IPropertySymbol prop) {
                // Raw states are excluded from the generation process
                if (symbol.GetDerivedAttribute<RawStateAttribute>(semanticModel) != null) {
                    continue;
                }

                return (prop.OriginalDefinition, statePropName);
            }
        }

        return null;
    }

    private static string GenerateTypeExtension(INamedTypeSymbol containingType, INamedTypeSymbol type, IEnumerable<IGrouping<ISymbol?, string>> propGroups) {
        var template =
            """
            {5}

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            [System.CodeDom.Compiler.GeneratedCode("Reactive_StateGenerator", "1.0")]
            internal static class Reactive_{0}_StateGenExt {{
                extension{3}({1} obj) {4} {{
                    {2}
                }}
            }}
            """;

        var typeIdentifier = type.GetTypeIdentifier();
        var propertyBlocks = GenerateProps(type, propGroups);

        var genericArguments = CompilerHelper.GenerateGenericsDecl(containingType, true);
        var genericConstrainments = CompilerHelper.GenerateConstrainments(containingType);
        var imports = CompilerHelper.GenerateImportOf(containingType);

        template = string.Format(
            template,
            typeIdentifier,
            type,
            propertyBlocks,
            genericArguments,
            genericConstrainments,
            imports
        );

        return template.Insert(0, "\t\t").Replace("\n", "\n\t\t");
    }

    private static string GenerateProps(INamedTypeSymbol type, IEnumerable<IGrouping<ISymbol?, string>> propGroups) {
        var buffer = new StringBuilder();
        var binder = StateGeneratorUtils.IsUnityObject(type) ? "AddCallbackUnity" : "AddCallback";

        foreach (var nameGroup in propGroups) {
            var prop = nameGroup.Key!;
            var propType = SemanticExtensions.GetReturnType(prop)!;
            var sourcePropName = prop.Name;

            foreach (var propName in nameGroup) {
                var propExtension = GenerateProp(propType, propName, sourcePropName, binder);

                buffer.AppendLine(propExtension);
            }
        }

        return buffer.ToString();
    }

    private static string GenerateProp(ITypeSymbol stateType, string propName, string sourcePropName, string binderMethodName) {
        var definition =
            """
            [Reactive.Compiler.SetsRequiredAttribute(Names = ["{4}"])]
            public {0}<{2}, {1}<{2}>> {3} {{
                set {{
                    value.{5}(obj, static (x, y) => x.{4} = y);
                }}
            }}

            """;

        definition = string.Format(
            definition,
            StateGeneratorUtils.StateBinderPath, // StateBinder<>
            StateGeneratorUtils.StatePath,       // IState<>
            stateType,                           // State target type (State<T>)
            propName,                            // Prop name
            sourcePropName,                      // Target prop name
            binderMethodName                     // Binder name (e.g. AddCallback)
        );

        return definition;
    }
}