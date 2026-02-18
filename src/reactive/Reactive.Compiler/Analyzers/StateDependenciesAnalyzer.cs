using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class StateDependenciesAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor NonStateValuePassedRule = new(
        "RV101",
        "Dependency must be an IState instance",
        "\'{0}\' is not a valid dependency",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NonStateValuePassedRule);

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeDependencies, SyntaxKind.TupleExpression);
    }

    private static void AnalyzeDependencies(SyntaxNodeAnalysisContext context) {
        if (!IsDependenciesTuple(context.SemanticModel, context.Node)) {
            return;
        }

        var tupleItems = context.Node
            .ChildNodes()
            .OfType<ArgumentSyntax>()
            .ToArray();

        var validItems = SelectStateArguments(context.SemanticModel, tupleItems);
        
        foreach (var node in tupleItems.Except(validItems)) {
            var diagnostic = Diagnostic.Create(NonStateValuePassedRule, node.GetLocation(), node.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<SyntaxNode> SelectStateArguments(SemanticModel semanticModel, IEnumerable<SyntaxNode> arguments) {
        foreach (var argument in arguments) {
            var ident = argument.ChildNodes()
                .OfType<IdentifierNameSyntax>()
                .FirstOrDefault();

            if (ident == null) {
                continue;
            }
            
            var symbol = semanticModel.GetSymbolInfo(ident).Symbol;
            var returnType = SyntaxExtensions.GetReturnType(symbol);

            if (returnType.IsStateType()) {
                yield return argument;
            }
        }
    }

    private static bool IsDependenciesTuple(SemanticModel semanticModel, SyntaxNode node) {
        if (node.Parent is not ArgumentSyntax argument) {
            return false;
        }

        var invocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>()!;
        var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol!;

        // Determine the index of the potential dependency list
        var parameterIndex = invocation
            .DescendantNodes()
            .OfType<ArgumentListSyntax>()
            .First()
            .ChildNodesAndTokens()
            .TakeWhile(x => x != argument)
            .Count(x => x.IsKind(SyntaxKind.CommaToken));

        var parameterSymbol = methodSymbol.Parameters[parameterIndex];
        var attr = parameterSymbol.GetAttribute(semanticModel, CompilerHelper.StateDependenciesAttrPath);

        return attr != null;
    }
}