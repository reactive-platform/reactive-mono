using Microsoft.CodeAnalysis;

namespace Reactive.Compiler;

partial class SemanticExtensions {
    /// <summary>
    /// Returns the return type of the symbol if presented, otherwise null.
    /// </summary>
    public static ITypeSymbol? GetReturnType(ISymbol? symbol) {
        return symbol switch {
            IMethodSymbol method => method.ReturnType,
            IFieldSymbol field => field.Type,
            ILocalSymbol local => local.Type,
            IPropertySymbol property => property.Type,
            _ => null
        };
    }
    
    /// <summary>
    /// Returns a target extension type if this property is defined in an extension block.
    /// </summary>    
    public static ITypeSymbol? GetExtensionType(this IPropertySymbol prop) {
        return prop.ContainingType.ExtensionParameter?.Type;
    }
    
    /// <summary>
    /// Returns an extension type for the method if it's defined in an extension block
    /// or has <c>this T</c> as the first argument.
    /// </summary>
    public static ITypeSymbol? GetExtensionType(this IMethodSymbol method) {
        if (method.ContainingType.ExtensionParameter?.Type is { } blockType) {
            return blockType;
        }
        
        return method is not { IsExtensionMethod: true } ? null : method.Parameters.First().Type;
    }
}