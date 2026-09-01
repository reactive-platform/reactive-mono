using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

partial class RequiredAnalyzer {
    private static void AnalyzeSetsDeclaration(SyntaxNodeAnalysisContext context) {
        var node = (PropertyDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(node)!;

        var requiredAttr = symbol.GetAttribute<RequiredAttribute>(context.SemanticModel);
        var setsAttr = symbol.GetAttribute<SetsRequiredAttribute>(context.SemanticModel);

        if (setsAttr == null) {
            return;
        }

        if (requiredAttr != null) {
            var diagnostic = Diagnostic.Create(
                RequiredAndSetsRequiredRule,
                node.Identifier.GetLocation(),
                node.Identifier
            );

            context.ReportDiagnostic(diagnostic);
            return;
        }

        var type = symbol.GetExtensionType() ?? symbol.ContainingType;

        var setsParam = setsAttr.GetNamedArgument(nameof(SetsRequiredAttribute.Names));
        var setsMembers = setsParam?.Values
            .Select(y => y.Value)
            .OfType<string>();

        if (setsMembers == null) {
            return;
        }

        var requiredMembers = type.GetMembersRecursive()
            .OfType<IPropertySymbol>()
            .Where(x => x.GetAttribute<RequiredAttribute>(context.SemanticModel) != null)
            .Select(x => x.Name);

        var location = GetNamesArgSyntax(node).GetLocation();

        // Excluding presented properties and reporting on the remaining properties
        foreach (var name in setsMembers.Except(requiredMembers)) {
            var diagnostic = Diagnostic.Create(
                SetRequiredPropMissingRule,
                location,
                name,
                type.ToDisplayString()
            );

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static AttributeArgumentSyntax GetNamesArgSyntax(PropertyDeclarationSyntax node) {
        var attr = node.AttributeLists
            .SelectMany(x => x.Attributes)
            .First(x => x.Name.ToString().Contains("SetsRequired"));

        return attr.ArgumentList!.Arguments
            .First(x => x.NameEquals!.ToString().Contains(nameof(SetsRequiredAttribute.Names)));
    }
}