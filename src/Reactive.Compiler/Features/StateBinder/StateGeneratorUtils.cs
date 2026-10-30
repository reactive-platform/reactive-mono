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

    public const string LifetimeProviderPath = "Reactive.ILifetimeProvider";

    public static bool IsLifetimeProvider(ITypeSymbol symbol) {
        return symbol.ToDisplayString() == LifetimeProviderPath ||
            symbol.AllInterfaces.Any(x => x.ToDisplayString() == LifetimeProviderPath);
    }
    
    public static string GetBinderMethod(ITypeSymbol symbol) {
        if (IsUnityObject(symbol)) {
            return "AddCallbackUnity";
        }

        // Objects without a lifetime of their own live as long as they're referenced
        return IsLifetimeProvider(symbol) ? "AddCallback" : "AddCallbackWeak";
    }

    public static bool IsUnityObject(ITypeSymbol symbol) {
        var current = symbol;
        
        while (current != null) {
            if (current.ToDisplayString() == "UnityEngine.Object") {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    public static bool IsStateType(this ISymbol? symbol) {
        return symbol is ITypeSymbol type && (IsStateTypeSelf(type) || type.AllInterfaces.Any(IsStateTypeSelf));
    }

    private static bool IsStateTypeSelf(ITypeSymbol symbol) {
        return symbol.ContainingNamespace?.ToString() == StateNamespace && symbol.Name == StateType;
    }
}