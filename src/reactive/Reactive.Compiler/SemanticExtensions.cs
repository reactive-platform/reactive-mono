using Microsoft.CodeAnalysis;

namespace Reactive.Compiler;

internal static class SemanticExtensions {
    public static string GetTypeIdentifier(this ISymbol type) {
        return type
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace(".", "_")
            .Replace("<", "_")
            .Replace(">", "")
            .Replace(",", "_");
    }

    
    public static ITypeSymbol? GetExtensionType(this IPropertySymbol prop) {
        return prop.ContainingType.ExtensionParameter?.Type ??
            GetMethodInlineExtensionType(prop.GetMethod) ??
            GetMethodInlineExtensionType(prop.SetMethod);
    }
    
    /// <summary>
    /// Returns an extension type for the method. Works only on methods with <c>Method(this T)</c> syntax,
    /// extension blocks are not supported.
    /// </summary>
    private static ITypeSymbol? GetMethodInlineExtensionType(IMethodSymbol? method) {
        return method is not { IsExtensionMethod: true } ? null : method.Parameters.First().Type;

    }

    public static TypedConstant? GetNamedArgument(this AttributeData data, string argument) {
        var arg = data.NamedArguments.FirstOrDefault(x => x.Key == argument);
        return arg.Key == null ? null : arg.Value;
    }

    /// <summary>
    /// Acquires all members of a type and its supertypes that match the provided string.
    /// </summary>
    public static IEnumerable<ISymbol> GetMembersRecursive(this ITypeSymbol type, string? name = null) {
        var current = type;

        do {
            var members = name != null ? current.GetMembers(name) : current.GetMembers();

            foreach (var member in members) {
                yield return member;
            }

            current = current.BaseType;
        } while (current != null);
    }

    #region GetAttribute

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attribute) {
        return symbol
            .GetAttributes()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute));
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, SemanticModel semanticModel, string name) {
        var compilation = semanticModel.Compilation;
        var attribute = compilation.GetTypeByMetadataName(name);

        return GetAttribute(symbol, attribute!);
    }

    public static AttributeData? GetAttribute<T>(this ISymbol symbol, SemanticModel semanticModel) {
        var compilation = semanticModel.Compilation;
        var attribute = compilation.GetTypeByMetadataName(typeof(T).FullName!);

        return GetAttribute(symbol, attribute!);
    }

    #endregion

    #region GetDerivedAttribute

    public static AttributeData? GetDerivedAttribute(this IMethodSymbol symbol, SemanticModel semanticModel, string name) {
        var compilation = semanticModel.Compilation;
        var attribute = compilation.GetTypeByMetadataName(name);

        return GetDerivedAttribute(symbol, attribute!);
    }

    public static AttributeData? GetDerivedAttribute(this IMethodSymbol symbol, INamedTypeSymbol attribute) {
        IMethodSymbol? currentMethod = symbol;

        do {
            var attrs = currentMethod.GetAttributes();
            var attr = attrs.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute));

            if (attr != null) {
                return attr;
            }

            currentMethod = currentMethod.OverriddenMethod;
        } while (currentMethod != null);

        return null;
    }

    #endregion
}