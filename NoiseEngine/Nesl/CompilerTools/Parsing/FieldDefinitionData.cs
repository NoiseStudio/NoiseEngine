using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct FieldDefinitionData(
    TypeIdentifierToken TypeIdentifier,
    NameToken Name,
    NeslModifier Modifiers
);
