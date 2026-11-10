using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Reactive.Compiler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GeneratedBindingAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor BindingOutOfContextRule = new(
        "RV041",
        "Generated binding is used outside of its generation context",
        "'{0}' is a generated binding, but this usage doesn't generate it, so it depends on another usage to exist. Assign a state to it inside an object initializer of a [StateGen] member instead.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(BindingOutOfContextRule);

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context) {
        var operation = (IPropertyReferenceOperation)context.Operation;
        var prop = operation.Property;

        if (!IsGeneratedBinding(prop)) {
            return;
        }

        var semanticModel = operation.SemanticModel!;
        if (GeneratesBinding(operation.Syntax, prop, semanticModel)) {
            return;
        }

        var location = operation.Syntax is MemberAccessExpressionSyntax access ? access.Name.GetLocation() : operation.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostic.Create(BindingOutOfContextRule, location, prop.Name));
    }

    private static bool IsGeneratedBinding(IPropertySymbol prop) {
        // Extension properties are nested into an extension block, so walking up to the generated class
        for (var type = prop.ContainingType; type != null; type = type.ContainingType) {
            var generated = type.GetAttributes().Any(x =>
                x.AttributeClass?.ToDisplayString() == typeof(GeneratedCodeAttribute).FullName &&
                x.ConstructorArguments.FirstOrDefault().Value as string == StateGenerator.GeneratorName
            );

            if (generated) {
                return true;
            }
        }

        return false;
    }

    private static bool GeneratesBinding(SyntaxNode syntax, IPropertySymbol binding, SemanticModel semanticModel) {
        if (syntax.Parent is not AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment ||
            assignment.Left != syntax
        ) {
            return false;
        }

        // `new Foo { sB = ... }` or `new Foo { Another = { sB = ... } }`
        if (assignment.Parent is not InitializerExpressionSyntax {
                RawKind: (int)SyntaxKind.ObjectInitializerExpression or (int)SyntaxKind.CollectionInitializerExpression,
                Parent: BaseObjectCreationExpressionSyntax or AssignmentExpressionSyntax
            } initializer
        ) {
            return false;
        }

        if (!assignment.Right.IsStateExpression(semanticModel)) {
            return false;
        }

        if (StateGenerator.GetPatterns(initializer, semanticModel) is not { } patterns) {
            return false;
        }

        if (SyntaxExtensions.FindInitializerType(initializer, semanticModel) is not { } type) {
            return false;
        }

        if (StateGenerator.MatchBinding(assignment, patterns, type, semanticModel) is not { } source) {
            return false;
        }

        // The usage must generate this exact binding, not a same-named one for another property
        var sourceNames = binding.GetAttribute<SetsRequiredAttribute>(semanticModel)?.GetNamedArgument("Names");
        if (sourceNames is not { Values: [{ Value: string sourceName }] } || sourceName != source.Name) {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(GetReceiver(source.OriginalDefinition), GetReceiver(binding));
    }

    private static ITypeSymbol? GetReceiver(IPropertySymbol prop) {
        var type = prop.ContainingType;
        var receiver = type.IsExtension ? type.ExtensionParameter?.Type : type;

        return receiver?.OriginalDefinition;
    }
}
