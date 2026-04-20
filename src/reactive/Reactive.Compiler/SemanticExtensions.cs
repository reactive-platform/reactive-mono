using Microsoft.CodeAnalysis;

namespace Reactive.Compiler;

internal static partial class SemanticExtensions {
    public static string GetTypeIdentifier(this ISymbol type) {
        return type
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace(" ", "")
            .Replace("global::", "")
            .Replace(".", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_");
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
    
    public static AttributeData? GetDerivedAttribute(this ISymbol symbol, SemanticModel semanticModel, string name) {
        var attribute = semanticModel.Compilation.GetTypeByMetadataName(name);
        if (attribute == null) return null;

        ISymbol? currentSymbol = symbol;

        while (currentSymbol != null) {
            var attr = currentSymbol.GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute));

            if (attr != null) {
                return attr;
            }
            
            currentSymbol = currentSymbol switch {
                IMethodSymbol method => method.OverriddenMethod,
                IPropertySymbol prop => prop.OverriddenProperty,
                // Constructors can't be overriden
                _ => null 
            };
        }

        return null;
    }

    #endregion
}