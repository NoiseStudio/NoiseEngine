using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;

namespace NoiseEngine.Generator;

internal static class GeneratorHelpers {

    public static void GenerateUsings(StringBuilder builder, TypeDeclarationSyntax type) {
        foreach (
            UsingDirectiveSyntax u in type.ParentNodes().First(x => x is BaseNamespaceDeclarationSyntax).Parent!
            .ChildNodes().OfType<UsingDirectiveSyntax>()
        ) {
            builder.AppendLine(u.ToFullString());
        }
    }

    public static void GenerateNamespaceWithType(
        StringBuilder builder, TypeDeclarationSyntax type, string? additionalModifiers = null
    ) {
        builder.Append("namespace ").Append(type.ParentNodes()
            .OfType<BaseNamespaceDeclarationSyntax>().First().Name.GetText()).AppendLine(" {");

        // Type declaration.
        builder.AppendIndentation();
        AddModifiers(builder, type.Modifiers, additionalModifiers);

        if (type is ClassDeclarationSyntax)
            builder.Append("class ");
        else if (type is StructDeclarationSyntax)
            builder.Append("struct ");
        else if (type is InterfaceDeclarationSyntax)
            builder.Append("interface ");
        else if (type is RecordDeclarationSyntax)
            builder.Append("record ");
        else
            throw new NotImplementedException();

        builder.Append(type.Identifier.Text).AppendLine(" {");
    }

    public static void AddModifiers(StringBuilder builder, SyntaxTokenList modifiers, string? additional) {
        const string Partial = "partial";

        bool has = false;
        foreach (SyntaxToken modifier in modifiers) {
            if (modifier.ValueText == Partial && !has) {
                if (additional is not null)
                    builder.Append(additional).Append(' ');
                has = true;
            } else {
                has = has || modifier.ValueText == additional;
            }

            builder.Append(modifier.ValueText).Append(' ');
        }

        if (!has && additional is not null)
            builder.Append(additional).Append(' ');
    }

    public static bool AssertNotUsingInternalThings(StringBuilder builder, SyntaxNode node) {
        bool reported = false;
        foreach (IdentifierNameSyntax name in node.AllChildNodes().OfType<IdentifierNameSyntax>()) {
            if (!name.Identifier.Text.StartsWith(GeneratorConstants.InternalThings))
                continue;

            builder.Append("#error ").Append(name.Identifier.Text)
                .AppendLine(" is internal and is not part of the API. The use of this is not allowed.");
            reported = true;
        }
        return reported;
    }

}
