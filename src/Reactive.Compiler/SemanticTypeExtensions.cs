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
            IParameterSymbol param => param.Type,
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

    /// <summary>
    /// Returns true if the type derives from the base type, not counting the base type itself.
    /// </summary>
    public static bool InheritsFrom(this INamedTypeSymbol type, INamedTypeSymbol baseType) {
        for (var current = type.BaseType; current != null; current = current.BaseType) {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if the member implicitly implements a member of any interface of the type.
    /// </summary>
    public static bool ImplementsInterfaceMember(this INamedTypeSymbol type, ISymbol symbol) {
        foreach (var iface in type.AllInterfaces) {
            foreach (var member in iface.GetMembers(symbol.Name)) {
                if (SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), symbol)) {
                    return true;
                }
            }
        }

        return false;
    }
}
