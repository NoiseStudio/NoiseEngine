using System;
using System.Buffers.Binary;

namespace NoiseEngine.Serialization;

internal class SerializationWriterDelegationLittleEndian : SerializationWriterDelegation {

    public override void WriteUInt16(ushort obj) {
        Span<byte> span = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteUInt32(uint obj) {
        Span<byte> span = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteUInt64(ulong obj) {
        Span<byte> span = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt16(short obj) {
        Span<byte> span = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt32(int obj) {
        Span<byte> span = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteInt64(long obj) {
        Span<byte> span = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat16(Half obj) {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteHalfLittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat32(float obj) {
        Span<byte> span = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(span, obj);
        Data.AddRange(span);
    }

    public override void WriteFloat64(double obj) {
        Span<byte> span = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(span, obj);
        Data.AddRange(span);
    }

}
