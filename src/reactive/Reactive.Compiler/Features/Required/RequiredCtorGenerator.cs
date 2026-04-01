using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

/// <summary>
/// Generates a public dummy constructor on any type that uses <see cref="RequiredAttribute"/> for its properties.
/// This is essential to ensure that <c>where T: new() </c> constrainment cannot be violated for such types.
/// </summary>
[Generator]
internal class RequiredCtorGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is PropertyDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node);

                    var attr = symbol?.GetAttribute<RequiredAttribute>(ctx.SemanticModel);
                    var arg = attr?.GetNamedArgument(nameof(RequiredAttribute.ShadowsName));

                    // Shadowing means that the prop is not defined in the class directly (e.g. an extension)
                    if (arg.HasValue) {
                        return null;
                    }

                    var node = ctx.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                    var typeSymbol = ctx.SemanticModel.GetDeclaredSymbol(node!);

                    return typeSymbol;
                }
            )
            .Collect()
            .SelectMany((x, _) => x
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                // Abstract classes cannot be constructed directly, so it's okay to omit the constructor
                .Where(y => !y.IsAbstract)
            );

        context.RegisterSourceOutput(candidates, (spc, type) => {
            var source = GenerateCtor(type!);
            var name = $"{type!.GetTypeIdentifier()}_RequiredCtor.g.cs";

            spc.AddSource(name, source);
        });
    }
    
    private static string GenerateCtor(ISymbol type) {
        var definition = """
                {0}

                [System.CodeDom.Compiler.GeneratedCode("Reactive_RequiredCtorGenerator", "1.0")]
                partial class {1} {{
                    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                    public {1}(Reactive.Compiler.Nothing _ = default) {{ }}
                }}
            """;

        var namespaceName = type.ContainingNamespace.IsGlobalNamespace
            ? ""
            : $"namespace {type.ContainingNamespace.ToDisplayString()};";

        return string.Format(
            definition,
            namespaceName,
            type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
        );
    }
}