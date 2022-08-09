using System;
using System.Buffers.Binary;

namespace NoiseEngine.Serialization;

internal class SerializationWriterDelegationBigEndian : SerializationWriterDelegation {

    public override void WriteUInt16(ushort obj) {
        Span<byte> span = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteUInt32(uint obj) {
        Span<byte> span = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteUInt64(ulong obj) {
        Span<byte> span = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt16(short obj) {
        Span<byte> span = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt32(int obj) {
        Span<byte> span = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt64(long obj) {
        Span<byte> span = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat16(Half obj) {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteHalfBigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat32(float obj) {
        Span<byte> span = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat64(double obj) {
        Span<byte> span = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleBigEndian(span, obj);
        Data.AddRange(span);
    }

}
