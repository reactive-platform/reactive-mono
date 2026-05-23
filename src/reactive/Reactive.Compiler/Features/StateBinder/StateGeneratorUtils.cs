using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

internal static class StateGeneratorUtils {
    public const string StateType = "IState";
    public const string StateNamespace = "Reactive";

    public const string StatePath = $"{StateNamespace}.{StateType}";
    public const string StateBinderPath = "Reactive.Compiler.StateBinder";
    public const string AttributePath = "Reactive.Compiler.StateGenAttribute";

    public static ITypeSymbol? GetStateTargetType(ITypeSymbol symbol) {
        INamedTypeSymbol? type;

        if (symbol is INamedTypeSymbol t && IsStateTypeSelf(symbol)) {
            type = t;
        } else {
            type = symbol.AllInterfaces.FirstOrDefault(IsStateTypeSelf);
        }

        return type?.TypeArguments.First();
    }

    public static bool IsStateExpression(this ExpressionSyntax node, SemanticModel semanticModel) {
        return SyntaxExtensions.BuildAccessTree(node)
            .Select(x => (x, semanticModel.GetSymbolInfo(x).Symbol))
            .Where(x => x.Symbol != null)
            .Select(x => SemanticExtensions.GetReturnType(x.Symbol!))
            .Any(x => x.IsStateType());
    }

    public static bool IsStateType(this ISymbol? symbol) {
        return symbol is ITypeSymbol type && (IsStateTypeSelf(type) || type.AllInterfaces.Any(IsStateTypeSelf));
    }

    private static bool IsStateTypeSelf(ITypeSymbol symbol) {
        return symbol.ContainingNamespace?.ToString() == StateNamespace && symbol.Name == StateType;
    }
}