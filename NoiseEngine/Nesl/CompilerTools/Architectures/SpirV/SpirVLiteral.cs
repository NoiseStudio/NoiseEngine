using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal readonly record struct SpirVLiteral(IReadOnlyList<byte> Bytes) {

    public ushort WordCount => (ushort)(Bytes.Count / 4);

    public static SpirVLiteral operator +(SpirVLiteral a, SpirVLiteral b) {
        return new SpirVLiteral(a.Bytes.Concat(b.Bytes).ToArray());
    }

}
