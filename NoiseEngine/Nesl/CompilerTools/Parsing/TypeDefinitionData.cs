using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct TypeDefinitionData(
    CodePointer Pointer,
    NeslTypeBuilder TypeBuilder,
    IReadOnlyList<InheritanceToken> Inheritances,
    IReadOnlyList<ConstraintToken> Constraints,
    TokenBuffer Buffer
);
