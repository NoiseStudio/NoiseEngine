using System;
using System.Collections.Generic;

namespace NoiseEngine.Serialization;

internal abstract class SerializationReaderDelegationArray : SerializationReaderDelegation {

    protected readonly byte[] data;

    public override int Count => data.Length;
    public override byte this[int index] => data[index];

    protected SerializationReaderDelegationArray(SerializationReader reader, byte[] data) : base(reader) {
        this.data = data;
    }

    public override void ReadBytes(Span<byte> bytes) {
        data.AsSpan(Reader.Position, bytes.Length).CopyTo(bytes);
    }

    public override byte[] ToArray() {
        return data.AsSpan().ToArray();
    }

    public override IEnumerator<byte> GetEnumerator() {
        foreach (byte b in data)
            yield return b;
    }

    protected ReadOnlySpan<byte> ReadAndMovePosition(int size) {
        ReadOnlySpan<byte> span = data.AsSpan(Reader.Position, size);
        Reader.Position += size;
        return span;
    }

}
