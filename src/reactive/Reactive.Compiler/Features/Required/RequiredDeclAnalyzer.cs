using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

partial class RequiredAnalyzer {
    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context) {
        var node = (PropertyDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node)!;

        if (symbol.GetAttribute<RequiredAttribute>(context.SemanticModel) == null) {
            return;
        }

        DiagnosticDescriptor? descriptor = null;

        if (symbol.ContainingType.IsStatic) {
            descriptor = RequiredStaticClassRule;
        } else if (symbol.IsStatic) {
            descriptor = RequiredStaticPropRule;
        }

        if (descriptor != null) {
            var diagnostic = Diagnostic.Create(
                descriptor,
                node.Identifier.GetLocation(),
                node.Identifier
            );
            
            context.ReportDiagnostic(diagnostic);
        }
    }
}