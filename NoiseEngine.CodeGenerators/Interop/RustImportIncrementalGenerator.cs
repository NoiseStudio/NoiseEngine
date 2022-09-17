using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NoiseEngine.CodeGenerators.Interop;

[Generator]
public class RustImportIncrementalGenerator : IIncrementalGenerator {

    private const string DllName = "NoiseEngine.Native";
    private const string AttributeFullName = "NoiseEngine.Interop.RustImportAttribute";

    private static readonly Dictionary<string, RustMarshaller> marshallers = new Dictionary<string, RustMarshaller>();

    static RustImportIncrementalGenerator() {
        foreach (
            Type type in typeof(RustImportIncrementalGenerator).Assembly.GetTypes()
            .Where(x => typeof(RustMarshaller).IsAssignableFrom(x) && x != typeof(RustMarshaller))
        ) {
            RustMarshaller marshaller = (RustMarshaller)Activator.CreateInstance(type);
            marshallers.Add(marshaller.MarshalledType, marshaller);
        }
    }

    private static string SplitWithGenerics(string fullName, out string genericRawString) {
        int index = fullName.IndexOf('<');

        if (index == -1) {
            genericRawString = string.Empty;
            return fullName;
        }

        genericRawString = fullName.Substring(index + 1, fullName.Length - index - 2);
        return fullName.Substring(0, index);
    }

    private static void AddModifiers(StringBuilder builder, SyntaxTokenList modifiers) {
        const string Partial = "partial";
        const string Unsafe = "unsafe";

        bool hasUnsafe = false;
        foreach (SyntaxToken modifier in modifiers) {
            if (modifier.ValueText == Partial && !hasUnsafe) {
                builder.Append(Unsafe).Append(' ');
                hasUnsafe = true;
            } else {
                hasUnsafe |= modifier.ValueText == Unsafe;
            }

            builder.Append(modifier.ValueText).Append(' ');
        }

        if (!hasUnsafe)
            builder.Append(Unsafe).Append(' ');
    }

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValuesProvider<(MethodDeclarationSyntax, AttributeSyntax)> methods = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (x, _) => x is MethodDeclarationSyntax m && m.AttributeLists.Any(),
                static (context, _) => {
                    MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
                    foreach (AttributeSyntax attribute in method.AttributeLists.SelectMany(x => x.Attributes)) {
                        if (
                            context.SemanticModel.GetSymbolInfo(attribute).Symbol
                            is not IMethodSymbol attributeSymbol
                        ) {
                            continue;
                        }

                        if (attributeSymbol.ContainingType.ToDisplayString() == AttributeFullName)
                            return (method, attribute);
                    }

                    return (method, null!);
                }
            ).Where(static x => x.attribute is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<(MethodDeclarationSyntax, AttributeSyntax)>)>
            compilationAndMethods = context.CompilationProvider.Combine(methods.Collect());

        context.RegisterSourceOutput(compilationAndMethods, (context, source) => {
            if (source.Item2.IsDefaultOrEmpty)
                return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("// AUTO GENERATED - DO NOT EDIT");
            builder.AppendLine();

            foreach ((MethodDeclarationSyntax method, AttributeSyntax attribute) in source.Item2)
                GenerateExtension(builder, source.Item1, method, attribute);

            context.AddSource("RustImport.generated.cs", builder.ToString());
        });
    }

    private void GenerateExtension(
        StringBuilder builder, Compilation compilation, MethodDeclarationSyntax method, AttributeSyntax attribute
    ) {
        // Namespace declaration.
        builder.Append("namespace ").Append(method.ParentNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>().First().Name.GetText()).AppendLine(" {");

        // Type declaration.
        TypeDeclarationSyntax type = method.ParentNodes().OfType<TypeDeclarationSyntax>().First();

        builder.AppendIndentation();
        AddModifiers(builder, type.Modifiers);

        if (type is ClassDeclarationSyntax)
            builder.Append("class ");
        else if (type is StructDeclarationSyntax)
            builder.Append("struct ");
        else if (type is InterfaceDeclarationSyntax)
            builder.Append("interface ");
        else if (type is RecordDeclarationSyntax)
            builder.Append("record ");
        else
            builder.Append("record struct ");

        builder.Append(type.Identifier.Text).AppendLine(" {");

        // Method declaration.
        int attributeIndex = builder.Length - 1;

        builder.AppendIndentation(2);
        foreach (SyntaxToken modifier in method.Modifiers)
            builder.Append(modifier.ValueText).Append(' ');

        string returnTypeName = method.ReturnType.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString();
        builder.Append(returnTypeName).Append(' ');
        builder.Append(method.Identifier.ValueText);
        builder.Append('(');

        foreach (ParameterSyntax parameter in method.ParameterList.Parameters) {
            builder.Append(parameter.Type!.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString()).Append(' ');
            builder.Append(parameter.Identifier.ValueText);
            builder.Append(", ");
        }

        if (method.ParameterList.Parameters.Count > 0)
            builder.Remove(builder.Length - 2, 2);

        builder.Append(')');

        // Create method body.
        StringBuilder body = new StringBuilder();
        StringBuilder advancedBody = new StringBuilder(RustMarshaller.MarshallingContinuation);
        StringBuilder outputBody = new StringBuilder();
        List<(string, string)> parameters = new List<(string, string)>();
        List<(string, string, string)> outputs = new List<(string, string, string)>();

        foreach (ParameterSyntax parameter in method.ParameterList.Parameters) {
            string typeFullName = parameter.Type!.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString();
            string typeName = SplitWithGenerics(typeFullName, out string genericRawString);

            if (!marshallers.TryGetValue(typeName, out RustMarshaller marshaller)) {
                parameters.Add((parameter.Identifier.ValueText, typeFullName));
                continue;
            }

            marshaller.SetGenericRawString(genericRawString);
            string a = marshaller.Marshall(parameter.Identifier.ValueText, out string newParameterName);
            marshaller.SetGenericRawString(string.Empty);

            string finalType = marshaller.MarshalledType;
            if (genericRawString.Length > 0)
                finalType += $"<{genericRawString}>";

            parameters.Add((newParameterName, finalType));

            if (marshaller.IsAdvanced)
                advancedBody.Replace(RustMarshaller.MarshallingContinuation, a);
            else
                body.AppendLine(a);
        }

        // TODO: add out values.
        foreach (TypeSyntax typeSyntax in new TypeSyntax[] { method.ReturnType }) {
            string typeFullName = typeSyntax.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString();
            string typeName = SplitWithGenerics(typeFullName, out string genericRawString);

            string b = RustMarshaller.CreateUniqueVariableName();
            if (!marshallers.TryGetValue(typeName, out RustMarshaller marshaller)) {
                outputs.Add((b, b, typeFullName));
                continue;
            }

            marshaller.SetGenericRawString(genericRawString);
            outputBody.AppendLine(marshaller.Unmarshall(b, out string newParameterName));
            marshaller.SetGenericRawString(string.Empty);

            string finalType = marshaller.UnmarshalledType;
            if (genericRawString.Length > 0)
                finalType += $"<{genericRawString}>";

            outputs.Add((b, newParameterName, finalType));
        }

        // Create DllImportAttribute.
        StringBuilder dllImport = new StringBuilder("[System.Runtime.InteropServices.DllImportAttribute(");
        dllImport.Append(attribute.ArgumentList!.Arguments.Count == 2 ?
            attribute.ArgumentList.Arguments[1] : $"\"{DllName}\"");
        dllImport.Append(", EntryPoint = ");
        dllImport.Append(attribute.ArgumentList.Arguments[0]);
        dllImport.Append(", ExactSpelling = true)]");

        // Construct final method body.
        if (body.Length > 0 || advancedBody.ToString() != RustMarshaller.MarshallingContinuation) {
            builder.AppendLine(" {");

            // Create __PInvoke method.
            builder.AppendIndentation(3).Append(dllImport).AppendLine();
            builder.AppendIndentation(3).Append("static extern unsafe ");
            builder.Append(returnTypeName);
            builder.Append(" __PInvoke(");

            int i = 0;
            foreach ((string name, string typeString) in parameters) {
                builder.Append(typeString);
                builder.Append(" v");
                builder.Append(i++);
                builder.Append(", ");
            }

            if (parameters.Count > 0)
                builder.Remove(builder.Length - 2, 2);

            builder.AppendLine(");");

            // Append body.
            builder.Append(body).AppendLine();

            // Declare output variables.
            body.Clear();

            bool returnTypeIsNotVoid = returnTypeName != "void";
            if (returnTypeIsNotVoid) {
                (string, string, string) returnInfo = outputs[0];

                body.Append(returnInfo.Item3).Append(' ');
                body.Append(returnInfo.Item1).Append(';').AppendLine();
            }

            builder.Append(body);

            // Add PInvoke execution.
            body.Clear();

            if (returnTypeIsNotVoid) {
                (string, string, string) returnInfo = outputs[0];
                body.Append(returnInfo.Item1).Append(" = ");
            }

            body.Append("__PInvoke(");

            foreach ((string name, _) in parameters) {
                body.Append(name);
                body.Append(", ");
            }

            if (parameters.Count > 0)
                body.Remove(body.Length - 2, 2);

            body.Append(");");

            advancedBody.Replace(RustMarshaller.MarshallingContinuation, body.ToString());
            builder.Append(advancedBody).AppendLine();
            builder.Append(outputBody);

            // Collect output.
            for (i = 1; i < outputs.Count; i++) {
                (string, string, string) returnInfo = outputs[i];

                builder.Append(returnInfo.Item2);
                builder.Append(" = ");
                builder.Append(returnInfo.Item1).Append(';').AppendLine();
            }

            if (returnTypeIsNotVoid) {
                (string, string, string) returnInfo = outputs[0];

                builder.Append("return ");
                builder.Append(returnInfo.Item2).Append(';').AppendLine();
            }

            builder.AppendIndentation(2).Append('}').AppendLine();
        } else {
            builder.Insert(attributeIndex, GeneratorConstants.Indentation + GeneratorConstants.Indentation + dllImport);
            builder.Append(';');
        }

        builder.AppendIndentation().Append('}').AppendLine();
        builder.AppendLine("}");
    }

}
