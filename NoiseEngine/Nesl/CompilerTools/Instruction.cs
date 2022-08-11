using NoiseEngine.Nesl.Emit;
using System;
using System.Buffers.Binary;

namespace NoiseEngine.Nesl.CompilerTools;

internal readonly struct Instruction {

    private readonly IlContainer container;
    private readonly uint tailIndex;

    public OpCode OpCode { get; }

    internal Instruction(OpCode opCode, uint tailIndex, IlContainer container) {
        OpCode = opCode;
        this.tailIndex = tailIndex;
        this.container = container;
    }

    public float ReadFloat32() {
        return BinaryPrimitives.ReadSingleLittleEndian(GetTail());
    }

    public byte ReadUInt8() {
        return GetTail()[0];
    }

    public uint ReadUInt32() {
        return BinaryPrimitives.ReadUInt32LittleEndian(GetTail());
    }

    public ulong ReadUInt64() {
        return BinaryPrimitives.ReadUInt64LittleEndian(GetTail());
    }

    private ReadOnlySpan<byte> GetTail() {
        return container.GetTail((int)tailIndex);
    }

}
