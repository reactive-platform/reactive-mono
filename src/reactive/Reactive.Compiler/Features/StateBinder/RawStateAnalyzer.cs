using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RawStateAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor RawStateAgreementRule = new(
        "RV004",
        "By defining a raw state you take the responsibility of binding and unbinding passed state objects properly.",
        "Property {0} is a raw state and won't be used by the state generator. Please note that by defining a raw state you take the responsibility of binding and unbinding passed state objects properly.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor RawStateInvalidTypeRule = new(
        "RV005",
        "Raw states must be of type IState<T>",
        "Defining a raw state requires the property to implement IState<T>.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        RawStateAgreementRule,
        RawStateInvalidTypeRule
    );

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context) {
        var prop = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(prop) is not { } symbol) {
            return;
        }

        if (symbol.GetDerivedAttribute<RawStateAttribute>(context.SemanticModel) == null) {
            return;
        }
        
        context.ReportDiagnostic(Diagnostic.Create(RawStateAgreementRule, prop.GetLocation(), symbol.Name));

        // Raw states must implement IState
        if (!symbol.Type.IsStateType()) {
            context.ReportDiagnostic(Diagnostic.Create(RawStateInvalidTypeRule, prop.Type.GetLocation()));
        }
    }
}