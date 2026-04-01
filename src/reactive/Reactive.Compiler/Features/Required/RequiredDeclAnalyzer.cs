using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

partial class RequiredAnalyzer {
    private static void AnalyzeRequiredDeclaration(SyntaxNodeAnalysisContext context) {
        var node = (PropertyDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node)!;

        var attr = symbol.GetAttribute<RequiredAttribute>(context.SemanticModel);
        var shadowed = attr?.GetNamedArgument(nameof(RequiredAttribute.ShadowsName));

        if (!shadowed.HasValue) {
            return;
        }

        if (symbol.GetExtensionType() is not { } extensionType) {
            // Attribute is 100% defined at this point, so we just query by name
            var location = GetRequiredSyntax(node).GetLocation();
            var diagnostic = Diagnostic.Create(ShadowedOutsideExtensionRule, location);

            context.ReportDiagnostic(diagnostic);

            return;
        }

        var shadowedName = (string)shadowed.Value.Value!;
        var hasProperty = extensionType.GetMembers(shadowedName).OfType<IPropertySymbol>().Any();

        if (!hasProperty) {
            var required = GetRequiredSyntax(node);
            // Any assignment consists of NameEquals (or NameColor) and a second, 'value' token, 
            // so we simply acquire the last token in this syntax
            var shadowedArg = GetShadowedArgSyntax(required).ChildNodes().Last();
            var location = shadowedArg.GetLocation();

            var diagnostic = Diagnostic.Create(
                ShadowedPropMissingRule,
                location,
                shadowedName,
                extensionType.ToDisplayString()
            );

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static AttributeSyntax GetRequiredSyntax(PropertyDeclarationSyntax node) {
        return node.AttributeLists
            .SelectMany(x => x.Attributes)
            .First(x => x.Name.ToString() == "Required");
    }

    private static AttributeArgumentSyntax GetShadowedArgSyntax(AttributeSyntax node) {
        return node.ArgumentList!.Arguments
            .First(x => x.NameEquals!.ToString().Contains(nameof(RequiredAttribute.ShadowsName)));
    }
}