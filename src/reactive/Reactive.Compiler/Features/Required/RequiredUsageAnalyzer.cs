using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

partial class RequiredAnalyzer {
    private static void AnalyzeInitializer(SyntaxNodeAnalysisContext context) {
        // 1. Get all required properties
        // 2. Iterate through properties defined in initializer and load their semantics
        // 3. Remove all init properties from the initial list
        // 4. If the list is len > 0, show missing properties to the user
        var semanticModel = context.SemanticModel;
        var node = (ObjectCreationExpressionSyntax)context.Node;

        if (semanticModel.GetSymbolInfo(node).Symbol is not IMethodSymbol { ContainingType: { } type }) {
            return;
        }

        var required = GetRequiredProperties(semanticModel, type);

        // Parse initialized props only if there is any
        if (node.Initializer != null) {
            var assigned = node.Initializer.ChildNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(x => x.Left is IdentifierNameSyntax)
                .Select(x => semanticModel.GetSymbolInfo(x.Left).Symbol)
                .ToArray();

            // Properties set by another properties with SetsRequired attributes
            var setNames = assigned
                .Select(x => x!.GetAttribute<SetsRequiredAttribute>(semanticModel))
                .OfType<AttributeData>()
                .Select(x => x.GetNamedArgument(nameof(SetsRequiredAttribute.Names)))
                .SelectMany(x => x?.Values.Select(y => y.Value))
                .OfType<string>();
            
            // Required properties set directly
            var reqNames = assigned
                .Where(x => x!.GetAttribute<RequiredAttribute>(semanticModel) != null)
                .Select(x => x!.Name);

            // If property is assigned we remove it from the required list
            foreach (var name in setNames.Concat(reqNames)) {
                required.Remove(name);
            }
        }

        foreach (var name in required) {
            var diagnostic = Diagnostic.Create(RequiredInitPropsRule, node.Type.GetLocation(), name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static HashSet<string> GetRequiredProperties(SemanticModel semanticModel, ITypeSymbol type) {
        var members = type.GetMembersRecursive()
            .Where(x => x.GetAttribute<RequiredAttribute>(semanticModel) != null)
            .Select(x => x.Name);

        return new(members);
    }
}