using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal readonly record struct PropertyDefinitionData(
    TypeIdentifierToken TypeIdentifier, NameToken Name, bool HasSetter, bool HasInitializer, TokenBuffer? Getter,
    IReadOnlyList<NeslAttribute> GetterAttributes, TokenBuffer? Second, IReadOnlyList<NeslAttribute> SecondAttributes
);
