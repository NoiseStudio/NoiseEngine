using System;
using System.Buffers.Binary;

namespace NoiseEngine.Serialization;

internal class SerializationReaderDelegationArrayLittleEndian : SerializationReaderDelegationArray {

    public SerializationReaderDelegationArrayLittleEndian(SerializationReader reader, byte[] data)
        : base(reader, data) {
    }

    public override ushort ReadUInt16() {
        return BinaryPrimitives.ReadUInt16LittleEndian(ReadAndMovePosition(sizeof(ushort)));
    }

    public override uint ReadUInt32() {
        return BinaryPrimitives.ReadUInt32LittleEndian(ReadAndMovePosition(sizeof(uint)));
    }

    public override ulong ReadUInt64() {
        return BinaryPrimitives.ReadUInt64LittleEndian(ReadAndMovePosition(sizeof(ulong)));
    }

    public override short ReadInt16() {
        return BinaryPrimitives.ReadInt16LittleEndian(ReadAndMovePosition(sizeof(short)));
    }

    public override int ReadInt32() {
        return BinaryPrimitives.ReadInt32LittleEndian(ReadAndMovePosition(sizeof(int)));
    }

    public override long ReadInt64() {
        return BinaryPrimitives.ReadInt64LittleEndian(ReadAndMovePosition(sizeof(long)));
    }

    public override Half ReadFloat16() {
        return BinaryPrimitives.ReadHalfLittleEndian(ReadAndMovePosition(2));
    }

    public override float ReadFloat32() {
        return BinaryPrimitives.ReadSingleLittleEndian(ReadAndMovePosition(sizeof(float)));
    }

    public override double ReadFloat64() {
        return BinaryPrimitives.ReadDoubleLittleEndian(ReadAndMovePosition(sizeof(double)));
    }

}
