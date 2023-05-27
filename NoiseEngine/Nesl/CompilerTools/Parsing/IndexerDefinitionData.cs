using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct IndexerDefinitionData(
    TypeIdentifierToken TypeIdentifier,
    NameToken Name,
    TypeIdentifierToken IndexType,
    NameToken IndexName,
    TokenBuffer? Getter,
    IReadOnlyList<NeslAttribute> GetterAttributes,
    bool HasSetter,
    TokenBuffer? Setter,
    IReadOnlyList<NeslAttribute> SetterAttributes
);
