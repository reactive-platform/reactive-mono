using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class RequiredAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor RequiredPropsRule = new(
        "RV102",
        "All required fields must be initialized in order to instantiate the component",
        "Required property \'{0}\' must be initialized",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor SetRequiredPropMissingRule = new(
        "RV103",
        "The shadowed property you've specified doesn't exist in the target type",
        "Property \'{0}\' doesn't exist in type \'{1}\'",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor RequiredAndSetsRequiredRule = new(
        "RV104",
        "A property cannot be required and set required members simultaneously",
        "\'{0}\' property is required and sets required members which is not allowed",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor RequiredCtorRule = new(
        "RV107",
        "All required properties must be initialized in a public parameterless constructor",
        "Required property \'{0}\' must be initialized",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        RequiredPropsRule,
        SetRequiredPropMissingRule,
        RequiredAndSetsRequiredRule,
        RequiredCtorRule
    );

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeInitializer, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeRequiredDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeCtor, SyntaxKind.ConstructorDeclaration);
    }
}