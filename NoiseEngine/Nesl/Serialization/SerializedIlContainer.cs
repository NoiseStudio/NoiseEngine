using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedIlContainer : IlContainer {

    private readonly (OpCode opCode, uint tailIndex)[] rawInstructions;
    private readonly byte[] tail;

    protected override IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions => rawInstructions;

    public SerializedIlContainer(
        NeslAssembly assembly, (OpCode opCode, uint tailIndex)[] rawInstructions, byte[] tail
    ) : base(assembly) {
        this.rawInstructions = rawInstructions;
        this.tail = tail;
    }

    internal void ReplaceOpCode(int index, OpCode opCode) {
        rawInstructions[index].opCode = opCode;
    }

    internal Span<byte> GetWritableTail(int start) {
        return tail.AsSpan(start);
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

}
