using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools;

public abstract class IlContainer : ISerializable<IlContainer> {

    protected abstract IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions { get; }

    internal NeslAssembly Assembly { get; }
    internal IEnumerable<Instruction> Instructions => GetInstructions();

    protected IlContainer(NeslAssembly assembly) {
        Assembly = assembly;
    }

    /// <summary>
    /// Creates new <see cref="IlContainer"/> with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New <see cref="IlContainer"/> with data from <paramref name="reader"/>.</returns>
    public static IlContainer Deserialize(SerializationReader reader) {
        byte[] tail = new byte[reader.ReadInt32()];
        reader.ReadBytes(tail);

        (OpCode opCode, uint tailIndex)[] rawInstructions = new (OpCode, uint)[reader.ReadInt32()];
        for (int i = 0; i < rawInstructions.Length; i++)
            rawInstructions[i] = ((OpCode)reader.ReadUInt16(), reader.ReadUInt32());

        return new SerializedIlContainer(reader.GetFromStorage<NeslAssembly>(), rawInstructions, tail);
    }

    internal abstract ReadOnlySpan<byte> GetTail(int start);

    /// <summary>
    /// Serializes this <see cref="IlContainer"/> and writes it to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"><see cref="SerializationWriter"/>.</param>
    public void Serialize(SerializationWriter writer) {
        ReadOnlySpan<byte> tail = GetTail(0);
        writer.WriteInt32(tail.Length);
        writer.WriteBytes(tail);

        int start = writer.Count;
        writer.WriteInt32(0);

        int i = 0;
        foreach ((OpCode opCode, uint tailIndex) in RawInstructions) {
            writer.WriteUInt16((ushort)opCode);
            writer.WriteUInt32(tailIndex);
            i++;
        }

        Span<byte> span = writer.AsSpan(start);
        if (writer.IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);
    }

    internal IEnumerable<Instruction> GetInstructions() {
        foreach ((OpCode opCode, uint tailIndex) in RawInstructions)
            yield return new Instruction(opCode, tailIndex, this);
    }

    internal IEnumerable<(OpCode opCode, uint tailIndex)> GetRawInstructions() {
        return RawInstructions;
    }

}
