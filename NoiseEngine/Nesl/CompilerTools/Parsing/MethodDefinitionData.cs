using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct MethodDefinitionData(
    NeslModifiers Modifiers, TypeIdentifierToken? TypeIdentifier, NameToken Name, TokenBuffer Parameters,
    TokenBuffer? CodeBlock, IReadOnlyList<NeslAttribute> Attributes
) {

    public bool IsConstructor => TypeIdentifier is null;
    public bool IsAbstract => CodeBlock is null;

}
