using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Diagnostics;
using System.IO;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class TypeDeclaration : ParserExpressionContainer {

    public TypeDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    //[ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeKind)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        ModifiersToken modifiers, TypeKindToken typeKind, NameToken name,
        CurlyBracketsToken codeBlock
    ) {
        string fullName = Path.GetDirectoryName(name.Pointer.Path) ?? "";
        if (Parser.AssemblyPath.Length > 0 && fullName.StartsWith(Parser.AssemblyPath))
            fullName = fullName[Parser.AssemblyPath.Length..];
        while (fullName.StartsWith('/') || fullName.StartsWith('\\'))
            fullName = fullName[1..];
        while (fullName.EndsWith('/') || fullName.EndsWith('\\'))
            fullName = fullName[..^1];
        fullName = fullName.Replace('/', '.').Replace('\\', '.');
        fullName = $"{Assembly.Name}.{(fullName.Length > 0 ? fullName + '.' + name.Name : name.Name)}";

        bool successful = true;
        if (!Assembly.TryDefineType(fullName, out NeslTypeBuilder? typeBuilder)) {
            Parser.Throw(new CompilationError(name.Pointer, CompilationErrorType.TypeAlreadyExists, fullName));
            successful = false;
        }

        if (!successful)
            return;
        if (typeBuilder is null)
            throw new UnreachableException();

        if (typeKind.TypeKind == NeslTypeKind.Struct)
            typeBuilder.AddAttribute(ValueTypeAttribute.Create());

        Parser.DefineType(typeBuilder!, codeBlock.Buffer);
    }

}
