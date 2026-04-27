using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

/// <summary>
/// Generates a public dummy constructor on any type that uses <see cref="RequiredAttribute"/> for its properties.
/// This is essential to ensure that <c>where T: new() </c> constrainment cannot be violated for such types.
/// </summary>
[Generator]
internal class RequiredCtorGenerator : IIncrementalGenerator {
    record struct CtorGenerationData(
        INamedTypeSymbol TypeSymbol,
        bool HasRequiredProperties,
        bool HasRequiredSuperclass
    );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var propertyCandidates = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is PropertyDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => TransformProperty(ctx)
            )
            .Where(x => x is not null)
            .Select((x, _) => new CtorGenerationData(x!, true, false))
            .Collect();

        var superclassCandidates = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => TransformSuperclass(ctx)
            )
            .Where(x => x is not null)
            .Select((x, _) => new CtorGenerationData(x!, false, true))
            .Collect();

        var candidates = propertyCandidates
            .Combine(superclassCandidates)
            .SelectMany(static (pair, _) => {
                // Flatten both lists into one sequence
                var allInfo = pair.Left.Concat(pair.Right);

                // Group by the Symbol and merge the booleans
                return allInfo
                    .GroupBy(x => x.TypeSymbol, SymbolEqualityComparer.Default)
                    .Select(group => new CtorGenerationData(
                        TypeSymbol: (INamedTypeSymbol)group.Key!,
                        HasRequiredProperties: group.Any(g => g.HasRequiredProperties),
                        HasRequiredSuperclass: group.Any(g => g.HasRequiredSuperclass)
                    ))
                    .Where(y => !y.TypeSymbol.IsAbstract);
            });

        context.RegisterSourceOutput(candidates, (spc, data) => {
            var source = GenerateCtor(data!);
            var name = $"{data.TypeSymbol.GetTypeIdentifier()}_RequiredCtor.g.cs";

            spc.AddSource(name, source);
        });
    }

    private static INamedTypeSymbol? TransformProperty(GeneratorSyntaxContext ctx) {
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node);

        // Non-required properties aren't handled
        var attr = symbol?.GetAttribute<RequiredAttribute>(ctx.SemanticModel);
        if (attr == null) {
            return null;
        }

        // Shadowing means that the prop is not defined in the class directly (e.g. an extension)
        var arg = attr.GetNamedArgument(nameof(RequiredAttribute.ShadowsName));
        if (arg.HasValue) {
            return null;
        }

        var node = ctx.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        var typeSymbol = ctx.SemanticModel.GetDeclaredSymbol(node!);

        return typeSymbol as INamedTypeSymbol;
    }

    private static INamedTypeSymbol? TransformSuperclass(GeneratorSyntaxContext ctx) {
        var symbol = (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)!;

        // Check if base class is presented and has required members.
        // Only checking a single level because it's assumed that superclasses
        // also rely on this generator, so we won't miss a hierarchy stage
        var hasRequiredMembers = symbol.BaseType?
            .GetMembers()
            .Any(x => x.GetDerivedAttribute<RequiredAttribute>(ctx.SemanticModel) != null)
            ?? false;
        
        if (!hasRequiredMembers) {
            return null;
        }

        return symbol;
    }

    private static string GenerateCtor(CtorGenerationData data) {
        var definition = """
            {0}

            [System.CodeDom.Compiler.GeneratedCode("Reactive_RequiredCtorGenerator", "1.0")]
            partial class {1} {{
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public {2}(Reactive.Compiler.Nothing _ = default) {3} {{ }}
            }}
        """;

        var type = data.TypeSymbol;
        
        var namespaceName = type.ContainingNamespace.IsGlobalNamespace
            ? ""
            : $"namespace {type.ContainingNamespace.ToDisplayString()};";
        
        var superCtor = data.HasRequiredSuperclass ? ": base(_)" : "";
        
        return string.Format(
            definition,
            namespaceName,
            type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            type.Name,
            superCtor
        );
    }
}