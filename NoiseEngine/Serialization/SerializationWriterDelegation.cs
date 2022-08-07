using NoiseEngine.Collections;
using System;

namespace NoiseEngine.Serialization;

internal abstract class SerializationWriterDelegation {

    internal FastList<byte> Data { get; } = new FastList<byte>();

    public abstract void WriteUInt16(ushort obj);
    public abstract void WriteUInt32(uint obj);
    public abstract void WriteUInt64(ulong obj);

    public abstract void WriteInt16(short obj);
    public abstract void WriteInt32(int obj);
    public abstract void WriteInt64(long obj);

    public abstract void WriteFloat16(Half obj);
    public abstract void WriteFloat32(float obj);
    public abstract void WriteFloat64(double obj);

}
