using System;
using System.Collections;
using System.Collections.Generic;

namespace NoiseEngine.Serialization;

internal abstract class SerializationReaderDelegation : IReadOnlyList<byte> {

    public abstract int Count { get; }
    public abstract byte this[int index] { get; }

    protected SerializationReader Reader { get; }

    protected SerializationReaderDelegation(SerializationReader reader) {
        Reader = reader;
    }

    public abstract ushort ReadUInt16();
    public abstract uint ReadUInt32();
    public abstract ulong ReadUInt64();

    public abstract short ReadInt16();
    public abstract int ReadInt32();
    public abstract long ReadInt64();

    public abstract Half ReadFloat16();
    public abstract float ReadFloat32();
    public abstract double ReadFloat64();

    public abstract void ReadBytes(Span<byte> bytes);
    public abstract byte[] ToArray();
    public abstract IEnumerator<byte> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
