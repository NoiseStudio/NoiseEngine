using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Utils;

internal readonly record struct GetterSetterUtilsResult(
    bool HasSetter, bool HasInitializer, TokenBuffer? Getter, IReadOnlyList<NeslAttribute> GetterAttributes,
    TokenBuffer? Second, IReadOnlyList<NeslAttribute> SecondAttributes
);
