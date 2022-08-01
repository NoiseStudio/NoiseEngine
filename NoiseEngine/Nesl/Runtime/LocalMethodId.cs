using System;
using System.Buffers.Binary;

namespace NoiseEngine.Nesl.Runtime;

internal readonly record struct LocalMethodId(ulong Id) {

    public void WriteBytes(Span<byte> bytes) {
        BinaryPrimitives.WriteUInt64BigEndian(bytes, Id);
    }

}
