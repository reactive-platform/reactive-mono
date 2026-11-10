using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler.OtherAnalyzers;

/// <summary>
/// Reports public methods declared on reactive components, which drive the component imperatively
/// instead of exposing its state through properties.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class IntentAntipatternAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor PublicMethodRule = new(
        "RV109",
        "Reactive component exposes a public method",
        "'{0}' is a public method of a reactive component. Using intent is an antipattern, expose its effect as a property or a state instead.",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PublicMethodRule);

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static startContext => {
            // Nothing to check when Reactive isn't referenced
            if (startContext.Compilation.GetTypeByMetadataName(TypeConstants.ReactiveComponent) is not { } componentType) {
                return;
            }

            startContext.RegisterSymbolAction(ctx => AnalyzeMethod(ctx, componentType), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol componentType) {
        if (context.Symbol is not IMethodSymbol {
                MethodKind: MethodKind.Ordinary,
                DeclaredAccessibility: Accessibility.Public,
                IsStatic: false,
                IsOverride: false,
                IsImplicitlyDeclared: false
            } method) {
            return;
        }

        var type = method.ContainingType;

        if (!InheritsFrom(type, componentType) || ImplementsInterfaceMember(type, method)) {
            return;
        }

        foreach (var location in method.Locations) {
            context.ReportDiagnostic(Diagnostic.Create(PublicMethodRule, location, method.Name));
        }
    }

    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType) {
        // The component itself is excluded, it defines the base API
        for (var current = type.BaseType; current != null; current = current.BaseType) {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType)) {
                return true;
            }
        }

        return false;
    }

    private static bool ImplementsInterfaceMember(INamedTypeSymbol type, IMethodSymbol method) {
        // The interface dictates the method, so it's not the component's choice to expose it
        foreach (var iface in type.AllInterfaces) {
            foreach (var member in iface.GetMembers(method.Name)) {
                if (SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), method)) {
                    return true;
                }
            }
        }

        return false;
    }
}