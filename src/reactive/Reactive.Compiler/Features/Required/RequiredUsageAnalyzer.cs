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
                .Select(x => semanticModel.GetSymbolInfo(x.Left).Symbol);

            foreach (var symbol in assigned) {
                // Checking whether this property shadows another one
                if (symbol!.GetAttribute<RequiredAttribute>(semanticModel) is { } attr) {
                    var shadowsName = attr.GetNamedArgument(nameof(RequiredAttribute.ShadowsName));

                    if (shadowsName != null) {
                        required.Remove((string)shadowsName.Value.Value!);

                        continue;
                    }
                }

                // If property is assigned we remove it from the required list
                required.Remove(symbol!.Name);
            }
        }

        foreach (var name in required) {
            var diagnostic = Diagnostic.Create(RequiredPropsRule, node.Type.GetLocation(), name);

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