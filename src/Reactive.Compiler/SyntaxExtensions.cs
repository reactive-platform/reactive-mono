using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reactive.Compiler;

internal static class SyntaxExtensions {
    public static SyntaxNode? GetEnclosingMember(this SyntaxNode node) {
        return node.Ancestors().FirstOrDefault(n =>
            n is MethodDeclarationSyntax or
                ConstructorDeclarationSyntax or
                PropertyDeclarationSyntax or
                AccessorDeclarationSyntax or
                IndexerDeclarationSyntax
        );
    }

    /// <summary>
    /// Attempts to find a type of the initializer expression.
    /// </summary>
    /// <returns></returns>
    public static ITypeSymbol? FindInitializerType(SyntaxNode syntax, SemanticModel model) {
        /// Not directly a syntax extension, but still relates to syntax more than to semantics

        var current = syntax;

        while (current != null) {
            switch (current) {
                // `new Foo { ... }`
                case ObjectCreationExpressionSyntax objectCreation:
                    return model.GetTypeInfo(objectCreation).Type;

                // `new Foo { Another = { ... } }`
                case AssignmentExpressionSyntax assignment:
                    var leftSymbol = model.GetSymbolInfo(assignment.Left).Symbol;

                    if (leftSymbol != null && SemanticExtensions.GetReturnType(leftSymbol) is { } returnType) {
                        return returnType;
                    }

                    break;

                // `{ ... }` itself
                case InitializerExpressionSyntax:
                    break;
            }

            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Walks through the expression building a hierarchy of access methods.
    /// </summary>
    public static IEnumerable<ExpressionSyntax> BuildAccessTree(ExpressionSyntax expression) {
        var stack = new Stack<ExpressionSyntax>();
        stack.Push(expression);

        while (stack.Count > 0) {
            var current = stack.Pop();
            yield return current;

            switch (current) {
                case InvocationExpressionSyntax invocation:
                    if (invocation.Expression is MemberAccessExpressionSyntax member) {
                        stack.Push(member.Expression);
                        yield return invocation;
                    }
                    break;
                
                case ConditionalExpressionSyntax ternary:
                    stack.Push(ternary.WhenFalse);
                    stack.Push(ternary.WhenTrue);
                    break;

                case PostfixUnaryExpressionSyntax unary:
                    stack.Push(unary.Operand);
                    break;

                case IdentifierNameSyntax identifier:
                    yield return identifier;
                    break;
                
                default:
                    // A shenanigan that allows to support every possible case
                    // without having to manually implement a case for each
                    var prop = current.GetType().GetProperty("Expression");
                    
                    if (prop?.GetValue(current) is ExpressionSyntax innerExpression) {
                        stack.Push(innerExpression);
                    }
                    
                    break;
            }
        }
    }
}