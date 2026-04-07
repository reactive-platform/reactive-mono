using System.Text;
using Microsoft.CodeAnalysis;

namespace Reactive.Compiler;

internal static class CompilerHelper {
    public const string StateDependenciesAttrPath = "Reactive.Compiler.StateDependenciesAttribute";

    public static string GenerateGenericsDecl(INamedTypeSymbol symbol) {
        if (symbol.TypeParameters.Any()) {
            return "<" + string.Join(", ", symbol.TypeParameters.Select(tp => tp.Name)) + ">";
        }

        return "";
    }

    public static string GenerateConstrainments(INamedTypeSymbol type) {
        var whereClauseBuilder = new StringBuilder();

        foreach (var typeParam in type.TypeParameters) {
            var constraints = new List<string>();

            // Check special constraints (must come first)
            if (typeParam.HasReferenceTypeConstraint) constraints.Add("class");
            if (typeParam.HasValueTypeConstraint) constraints.Add("struct");
            if (typeParam.HasNotNullConstraint) constraints.Add("notnull");
            if (typeParam.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");

            // Check for base class or interface constraints
            foreach (var constraintType in typeParam.ConstraintTypes) {
                constraints.Add(constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            // Check for the parameterless constructor constraint (must come last)
            if (typeParam.HasConstructorConstraint) {
                constraints.Add("new()");
            }

            // If we found any constraints, append the 'where' clause
            if (constraints.Count > 0) {
                whereClauseBuilder.Append($"where {typeParam.Name} : {string.Join(", ", constraints)} ");
            }
        }

        return whereClauseBuilder.ToString().Trim();
    }
}