using NoiseEngine.Nesl.Emit;
using System;
using System.Buffers.Binary;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools;

internal struct Instruction {

    private readonly IlContainer container;
    private uint tailIndex;

    public OpCode OpCode { get; }

    private NeslAssembly Assembly => container.Assembly;

    internal Instruction(OpCode opCode, uint tailIndex, IlContainer container) {
        OpCode = opCode;
        this.tailIndex = tailIndex;
        this.container = container;
    }

    public void OffsetTailIndex(int offset) {
        if (offset >= 0)
            tailIndex += (uint)offset;
        else
            tailIndex -= (uint)-offset;
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

    public override string ToString() {
        string? result = OpCode switch {
            OpCode.Add => $"{ReadUInt32()}u {ReadUInt32()}u {ReadUInt32()}u",
            OpCode.DefVariable => StringReadType(),
            OpCode.Call => $"{ReadUInt32()}u {StringReadMethod()}",
            OpCode.ReturnValue => $"{ReadUInt32()}u",
            _ => null
        };

        if (result is null)
            return OpCode.ToString();
        return $"{OpCode} {result}";
    }

    private ReadOnlySpan<byte> GetTail() {
        return container.GetTail((int)tailIndex);
    }

    private string StringReadType() {
        return Assembly.GetType(ReadUInt64()).ToString();
    }

    private string StringReadMethod() {
        StringBuilder builder = new StringBuilder(Assembly.GetMethod(ReadUInt64()).ToString());
        uint max = ReadUInt32();
        for (int i = 0; i < max; i++)
            builder.Append(' ').Append(ReadUInt32()).Append('u');

        return builder.ToString();
    }

}
