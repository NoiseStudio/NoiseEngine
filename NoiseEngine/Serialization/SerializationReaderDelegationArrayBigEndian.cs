using System;
using System.Buffers.Binary;

namespace NoiseEngine.Serialization;

internal class SerializationReaderDelegationArrayBigEndian : SerializationReaderDelegationArray {

    public SerializationReaderDelegationArrayBigEndian(SerializationReader reader, byte[] data)
        : base(reader, data) {
    }

    public override ushort ReadUInt16() {
        return BinaryPrimitives.ReadUInt16BigEndian(ReadAndMovePosition(sizeof(ushort)));
    }

    public override uint ReadUInt32() {
        return BinaryPrimitives.ReadUInt32BigEndian(ReadAndMovePosition(sizeof(uint)));
    }

    public override ulong ReadUInt64() {
        return BinaryPrimitives.ReadUInt64BigEndian(ReadAndMovePosition(sizeof(ulong)));
    }

    public override short ReadInt16() {
        return BinaryPrimitives.ReadInt16BigEndian(ReadAndMovePosition(sizeof(short)));
    }

    public override int ReadInt32() {
        return BinaryPrimitives.ReadInt32BigEndian(ReadAndMovePosition(sizeof(int)));
    }

    public override long ReadInt64() {
        return BinaryPrimitives.ReadInt64BigEndian(ReadAndMovePosition(sizeof(long)));
    }

    public override Half ReadFloat16() {
        return BinaryPrimitives.ReadHalfBigEndian(ReadAndMovePosition(2));
    }

    public override float ReadFloat32() {
        return BinaryPrimitives.ReadSingleBigEndian(ReadAndMovePosition(sizeof(float)));
    }

    public override double ReadFloat64() {
        return BinaryPrimitives.ReadDoubleBigEndian(ReadAndMovePosition(sizeof(double)));
    }

}
