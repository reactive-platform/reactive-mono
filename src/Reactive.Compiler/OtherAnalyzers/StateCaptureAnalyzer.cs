using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Reactive.Compiler.OtherAnalyzers;

/// <summary>
/// Reports states captured by lambdas passed to parameters marked with <see cref="NoStateCaptureAttribute"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class StateCaptureAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor StateCapturedRule = new(
        "RV108",
        "State is captured instead of being passed as a dependency",
        "'{0}' is a state captured by the closure, pass it as a dependency instead so its changes are tracked",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(StateCapturedRule);

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
    }

    private static void AnalyzeArgument(OperationAnalysisContext context) {
        var argument = (IArgumentOperation)context.Operation;

        if (argument.Parameter is not { } parameter || !HasAttribute(parameter)) {
            return;
        }

        if (GetLambda(argument.Value) is not { } lambda) {
            return;
        }

        var lambdaSpan = lambda.Syntax.Span;

        foreach (var operation in lambda.Body.Descendants()) {
            var (symbol, type) = operation switch {
                ILocalReferenceOperation local => ((ISymbol)local.Local, local.Local.Type),
                IParameterReferenceOperation param => (param.Parameter, param.Parameter.Type),
                IFieldReferenceOperation field => (field.Field, field.Field.Type),
                IPropertyReferenceOperation property => (property.Property, property.Property.Type),
                _ => (null, null)
            };

            if (symbol == null || !type.IsStateType()) {
                continue;
            }

            // Declared by the lambda itself, so it isn't captured
            if (symbol.DeclaringSyntaxReferences.Any(x => lambdaSpan.Contains(x.Span))) {
                continue;
            }

            // A member read through another expression, e.g. `x.State`, is reported on the receiver instead
            if (operation.Parent is IMemberReferenceOperation { Instance: var instance } && instance == operation && IsStateReference(operation.Parent)) {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(StateCapturedRule, operation.Syntax.GetLocation(), symbol.Name));
        }
    }

    private static bool IsStateReference(IOperation operation) {
        return operation switch {
            IFieldReferenceOperation field => field.Field.Type.IsStateType(),
            IPropertyReferenceOperation property => property.Property.Type.IsStateType(),
            _ => false
        };
    }

    private static IAnonymousFunctionOperation? GetLambda(IOperation operation) {
        // The lambda is wrapped in a delegate creation and possibly a conversion
        while (true) {
            switch (operation) {
                case IAnonymousFunctionOperation lambda:
                    return lambda;
                case IDelegateCreationOperation creation:
                    operation = creation.Target;
                    break;
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    break;
                default:
                    return null;
            }
        }
    }

    private static bool HasAttribute(IParameterSymbol parameter) {
        // Generated overloads and constructed generics keep the attribute on their definition
        return parameter.OriginalDefinition
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString() == typeof(NoStateCaptureAttribute).FullName);
    }
}
