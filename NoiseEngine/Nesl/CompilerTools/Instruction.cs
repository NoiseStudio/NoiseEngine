using NoiseEngine.Nesl.Emit;
using System;
using System.Buffers.Binary;

namespace NoiseEngine.Nesl.CompilerTools;

internal struct Instruction {

    private readonly IlContainer container;
    private uint tailIndex;

    public OpCode OpCode { get; }

    internal Instruction(OpCode opCode, uint tailIndex, IlContainer container) {
        OpCode = opCode;
        this.tailIndex = tailIndex;
        this.container = container;
    }

    public float ReadFloat32() {
        float result = BinaryPrimitives.ReadSingleLittleEndian(GetTail());
        tailIndex += sizeof(float);
        return result;
    }

    public byte ReadUInt8() {
        byte result = GetTail()[0];
        tailIndex += sizeof(byte);
        return result;
    }

    public uint ReadUInt32() {
        uint result = BinaryPrimitives.ReadUInt32LittleEndian(GetTail());
        tailIndex += sizeof(uint);
        return result;
    }

    public ulong ReadUInt64() {
        ulong result = BinaryPrimitives.ReadUInt64LittleEndian(GetTail());
        tailIndex += sizeof(ulong);
        return result;
    }

    private ReadOnlySpan<byte> GetTail() {
        return container.GetTail((int)tailIndex);
    }

}
