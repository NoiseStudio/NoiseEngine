using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct TypeDefinitionData(
    NeslTypeBuilder TypeBuilder,
    IReadOnlyList<TypeIdentifierToken> Inheritances,
    TokenBuffer Buffer
);
