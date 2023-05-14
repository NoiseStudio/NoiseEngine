using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

[Flags]
internal enum ParserStep {
    TopLevel = 1 << 0,
    Type = 1 << 1,
    Method = 1 << 2,
    Parameters = 1 << 3
}
