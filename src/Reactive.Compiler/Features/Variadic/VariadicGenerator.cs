using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

/// <summary>
/// Generates the overloads and types requested with <see cref="VariadicGenAttribute"/>.
/// </summary>
[Generator]
internal class VariadicGenerator : IIncrementalGenerator {
    private record struct GeneratedFile(string HintName, string Source);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var files = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(VariadicGenAttribute).FullName!,
            predicate: static (node, _) => node is MethodDeclarationSyntax or TypeDeclarationSyntax,
            transform: static (ctx, _) => Generate(ctx)
        );

        context.RegisterSourceOutput(files, static (spc, file) => {
            if (file is { } value) {
                spc.AddSource(value.HintName, value.Source);
            }
        });
    }

    private static GeneratedFile? Generate(GeneratorAttributeSyntaxContext ctx) {
        var attribute = ctx.Attributes[0];

        if (attribute.ConstructorArguments is not [{ Value: int maxArity }, { Kind: TypedConstantKind.Array } patternsArg]) {
            return null;
        }

        var patterns = patternsArg.Values.Select(x => x.Value).OfType<string>().ToArray();

        return ctx.TargetNode switch {
            MethodDeclarationSyntax method => GenerateMethods(ctx, method, patterns, maxArity),
            TypeDeclarationSyntax type => GenerateTypes(ctx, type, patterns, maxArity),
            _ => null
        };
    }

    private static GeneratedFile? GenerateMethods(GeneratorAttributeSyntaxContext ctx, MethodDeclarationSyntax method, string[] patterns, int maxArity) {
        // Only top-level partial types can receive members from another file
        if (method.Parent is not TypeDeclarationSyntax type || type.Parent is TypeDeclarationSyntax) {
            return null;
        }

        if (!type.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            return null;
        }

        var template = RemoveVariadicAttribute(method);
        var typeSymbol = ctx.TargetSymbol.ContainingType;
        var buffer = new StringBuilder();

        AppendHeader(buffer, ctx, method, typeSymbol.ContainingNamespace);

        buffer.AppendLine($"{(typeSymbol.IsStatic ? "static " : "")}partial {type.Keyword.Text} {type.Identifier.Text}{type.TypeParameterList} {{");

        for (var arity = 2; arity <= maxArity; arity++) {
            if (arity > 2) {
                buffer.AppendLine();
            }

            var overload = new Expander(patterns, arity).Visit(template)!;
            AppendIndented(buffer, overload.ToFullString(), "    ");
        }

        buffer.AppendLine("}");

        var hintName = $"{typeSymbol.Name}_{method.Identifier.Text}_{method.SpanStart}_Variadic.g.cs";

        return new GeneratedFile(hintName, buffer.ToString());
    }

    private static GeneratedFile? GenerateTypes(GeneratorAttributeSyntaxContext ctx, TypeDeclarationSyntax type, string[] patterns, int maxArity) {
        // Nested types would need their containing types declared as well
        if (type.Parent is TypeDeclarationSyntax) {
            return null;
        }

        var template = RemoveVariadicAttribute(type);
        var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var buffer = new StringBuilder();

        AppendHeader(buffer, ctx, type, typeSymbol.ContainingNamespace);

        for (var arity = 2; arity <= maxArity; arity++) {
            if (arity > 2) {
                buffer.AppendLine();
            }

            var copy = new Expander(patterns, arity).Visit(template)!;
            AppendIndented(buffer, copy.ToFullString(), "");
        }

        var hintName = $"{typeSymbol.Name}_{typeSymbol.Arity}_Variadic.g.cs";

        return new GeneratedFile(hintName, buffer.ToString());
    }

    /// <summary>
    /// Writes the nullable context, the template's usings and its namespace, so the copied code resolves the same way.
    /// </summary>
    private static void AppendHeader(StringBuilder buffer, GeneratorAttributeSyntaxContext ctx, SyntaxNode template, INamespaceSymbol ns) {
        if (ctx.SemanticModel.GetNullableContext(template.SpanStart).AnnotationsEnabled()) {
            buffer.AppendLine("#nullable enable");
            buffer.AppendLine();
        }

        var usings = template.Ancestors()
            .SelectMany(x => x switch {
                CompilationUnitSyntax unit => unit.Usings,
                BaseNamespaceDeclarationSyntax nsDecl => nsDecl.Usings,
                _ => []
            })
            .Where(x => x.GlobalKeyword.IsKind(SyntaxKind.None))
            .Select(x => x.ToString())
            .Distinct()
            .ToArray();

        foreach (var directive in usings) {
            buffer.AppendLine(directive);
        }

        if (usings.Length > 0) {
            buffer.AppendLine();
        }

        if (!ns.IsGlobalNamespace) {
            buffer.AppendLine($"namespace {ns.ToDisplayString()};");
            buffer.AppendLine();
        }
    }

    private static MemberDeclarationSyntax RemoveVariadicAttribute(MemberDeclarationSyntax member) {
        var lists = new List<AttributeListSyntax>();

        foreach (var list in member.AttributeLists) {
            var attributes = list.Attributes.Where(x => !IsVariadicAttribute(x)).ToArray();

            if (attributes.Length > 0) {
                lists.Add(list.WithAttributes(SyntaxFactory.SeparatedList(attributes)));
            }
        }

        // Keeping the doc comment, which lives in the leading trivia of the first token
        var trivia = member.GetLeadingTrivia();

        return member
            .WithAttributeLists(SyntaxFactory.List(lists))
            .WithLeadingTrivia(trivia);
    }

    private static bool IsVariadicAttribute(AttributeSyntax attribute) {
        var name = attribute.Name switch {
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            SimpleNameSyntax simple => simple.Identifier.Text,
            _ => attribute.Name.ToString()
        };

        const string FullName = nameof(VariadicGenAttribute);
        return name is FullName || FullName == name + "Attribute";
    }

    /// <summary>
    /// Re-indents the code to the given indentation, whatever indentation the template had.
    /// </summary>
    private static void AppendIndented(StringBuilder buffer, string code, string indent) {
        var lines = code.Replace("\r\n", "\n").Split('\n').ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1])) {
            lines.RemoveAt(lines.Count - 1);
        }

        var baseIndent = lines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Length - x.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        foreach (var line in lines) {
            buffer.AppendLine(string.IsNullOrWhiteSpace(line) ? "" : indent + line.Substring(baseIndent));
        }
    }

    /// <summary>
    /// Repeats the list elements matching the patterns, searching the rest for more of them.
    /// </summary>
    private sealed class Expander(string[] patterns, int arity) : CSharpSyntaxRewriter {
        private static readonly Regex whitespace = new(@"\s+");

        public override SyntaxNode? VisitTypeParameterList(TypeParameterListSyntax node) {
            return node.WithParameters(Expand(node.Parameters, SyntaxFactory.TypeParameter));
        }

        public override SyntaxNode? VisitParameterList(ParameterListSyntax node) {
            return node.WithParameters(Expand(node.Parameters, x => SyntaxFactory.ParseParameterList($"({x})").Parameters[0]));
        }

        public override SyntaxNode? VisitTypeArgumentList(TypeArgumentListSyntax node) {
            return node.WithArguments(Expand(node.Arguments, x => SyntaxFactory.ParseTypeName(x)));
        }

        public override SyntaxNode? VisitArgumentList(ArgumentListSyntax node) {
            return node.WithArguments(Expand(node.Arguments, x => SyntaxFactory.Argument(SyntaxFactory.ParseExpression(x))));
        }

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            // Members are expanded before the rest, so a repeated field isn't expanded from the inside first
            var members = ExpandMembers(node.Members);
            var visited = (ClassDeclarationSyntax)base.VisitClassDeclaration(node.WithMembers(default))!;

            return visited.WithMembers(members);
        }

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
            var members = ExpandMembers(node.Members);
            var visited = (StructDeclarationSyntax)base.VisitStructDeclaration(node.WithMembers(default))!;

            return visited.WithMembers(members);
        }

        public override SyntaxNode? VisitBlock(BlockSyntax node) {
            var statements = new List<StatementSyntax>();

            foreach (var statement in node.Statements) {
                if (Match(statement) is not { } pattern) {
                    statements.Add((StatementSyntax)Visit(statement)!);
                    continue;
                }

                for (var i = 1; i <= arity; i++) {
                    statements.Add(Repeat(statement, pattern.Replace("{}", i.ToString())));
                }
            }

            return node.WithStatements(SyntaxFactory.List(statements));
        }

        private SyntaxList<MemberDeclarationSyntax> ExpandMembers(SyntaxList<MemberDeclarationSyntax> list) {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var member in list) {
                if (Match(member) is not { } pattern) {
                    members.Add((MemberDeclarationSyntax)Visit(member)!);
                    continue;
                }

                for (var i = 1; i <= arity; i++) {
                    members.Add(Repeat(member, pattern.Replace("{}", i.ToString())));
                }
            }

            return SyntaxFactory.List(members);
        }

        /// <summary>
        /// Rebuilds the member or statement with the matched part replaced, keeping modifiers, trivia and the semicolon.
        /// </summary>
        private static T Repeat<T>(T node, string text) where T : SyntaxNode {
            SyntaxNode result = node switch {
                FieldDeclarationSyntax field => field.WithDeclaration(ParseDeclaration(text).WithTriviaFrom(field.Declaration)),
                LocalDeclarationStatementSyntax local => local.WithDeclaration(ParseDeclaration(text).WithTriviaFrom(local.Declaration)),
                ExpressionStatementSyntax statement => statement.WithExpression(SyntaxFactory.ParseExpression(text).WithTriviaFrom(statement.Expression)),
                StatementSyntax => SyntaxFactory.ParseStatement(text).WithTriviaFrom(node),
                _ => SyntaxFactory.ParseMemberDeclaration(text)!.WithTriviaFrom(node)
            };

            return (T)result;
        }

        private static VariableDeclarationSyntax ParseDeclaration(string text) {
            return ((LocalDeclarationStatementSyntax)SyntaxFactory.ParseStatement($"{text};")).Declaration;
        }

        /// <summary>
        /// The part of a node a pattern is compared to: fields and local declarations without modifiers and the semicolon,
        /// expression statements without the semicolon, anything else as a whole.
        /// </summary>
        private static string GetMatchText(SyntaxNode node) {
            return node switch {
                FieldDeclarationSyntax field => field.Declaration.ToString(),
                LocalDeclarationStatementSyntax local => local.Declaration.ToString(),
                ExpressionStatementSyntax statement => statement.Expression.ToString(),
                _ => node.ToString()
            };
        }

        private SeparatedSyntaxList<T> Expand<T>(SeparatedSyntaxList<T> list, Func<string, T> parse) where T : SyntaxNode {
            var items = new List<T>();

            foreach (var item in list) {
                if (Match(item) is not { } pattern) {
                    items.Add((T)Visit(item)!);
                    continue;
                }

                for (var i = 1; i <= arity; i++) {
                    items.Add(parse(pattern.Replace("{}", i.ToString())));
                }
            }

            var separators = Enumerable.Repeat(
                SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space),
                Math.Max(0, items.Count - 1)
            );

            return SyntaxFactory.SeparatedList(items, separators);
        }

        private string? Match(SyntaxNode node) {
            var text = whitespace.Replace(GetMatchText(node), "");

            return patterns.FirstOrDefault(x => whitespace.Replace(x.Replace("{}", "1"), "") == text);
        }
    }
}
