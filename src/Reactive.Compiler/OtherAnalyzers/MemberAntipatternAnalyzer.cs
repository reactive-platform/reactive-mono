using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler.OtherAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MemberAntipatternAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor PublicMethodRule = new(
        "RV109",
        "Reactive component exposes a public method",
        "'{0}' is a public method of a reactive component. Expose its effect as a property instead.",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor PublicEventRule = new(
        "RV110",
        "Reactive component exposes a public event",
        "'{0}' is a public event of a reactive component. Declare it as a delegate property instead.",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PublicMethodRule, PublicEventRule);

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static startContext => {
            // Nothing to check when Reactive isn't referenced
            if (startContext.Compilation.GetTypeByMetadataName(TypeConstants.ReactiveComponent) is not { } componentType) {
                return;
            }

            startContext.RegisterSymbolAction(ctx => AnalyzeMember(ctx, componentType), SymbolKind.Method, SymbolKind.Event);
        });
    }

    private static void AnalyzeMember(SymbolAnalysisContext context, INamedTypeSymbol componentType) {
        var symbol = context.Symbol;
        
        var rule = symbol switch {
            IMethodSymbol { MethodKind: MethodKind.Ordinary } => PublicMethodRule,
            IEventSymbol => PublicEventRule,
            _ => null
        };

        if (rule == null || symbol is not {
                DeclaredAccessibility: Accessibility.Public,
                IsStatic: false,
                IsOverride: false,
                IsImplicitlyDeclared: false
            }) {
            return;
        }

        var type = symbol.ContainingType;

        // The component itself is excluded since it defines the base API, and interfaces dictate their own members
        if (!type.InheritsFrom(componentType) || type.ImplementsInterfaceMember(symbol)) {
            return;
        }

        foreach (var location in symbol.Locations) {
            context.ReportDiagnostic(Diagnostic.Create(rule, location, symbol.Name));
        }
    }
}
