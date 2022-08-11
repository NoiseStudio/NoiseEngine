using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal readonly record struct SpirVLiteral(IReadOnlyList<byte> Bytes) {

    public ushort WordCount => (ushort)(Bytes.Count / 4);

}
