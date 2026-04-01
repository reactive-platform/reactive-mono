using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

partial class RequiredAnalyzer {
    private static void AnalyzeCtor(SyntaxNodeAnalysisContext context) {
        var node = (ConstructorDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node)!;

        if (symbol.ContainingType.IsAbstract || !IsPublicParameterless(symbol)) {
            return;
        }

        var requiredMembers = symbol.ContainingType
            .GetMembers()
            .Where(x => x.GetAttribute<RequiredAttribute>(context.SemanticModel) != null)
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        if (requiredMembers.Count is 0) {
            return;
        }

        var assignedMembers = node.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Select(x => context.SemanticModel.GetSymbolInfo(x.Left).Symbol)
            .OfType<ISymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        var location = node.Identifier.GetLocation();

        foreach (var prop in requiredMembers.Except(assignedMembers)) {
            var diagnostic = Diagnostic.Create(RequiredCtorRule, location, prop.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsPublicParameterless(IMethodSymbol constructor) {
        return constructor.Arity is 0 && constructor.DeclaredAccessibility is
            Accessibility.Public or
            Accessibility.Internal or
            Accessibility.ProtectedOrInternal;
    }
}