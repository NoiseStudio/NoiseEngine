using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedIlContainer : IlContainer {

    private readonly byte[] tail;

    protected override IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions { get; }

    public SerializedIlContainer(IEnumerable<(OpCode opCode, uint tailIndex)> rawInstructions, byte[] tail) {
        RawInstructions = rawInstructions;
        this.tail = tail;
    }

    internal Span<byte> GetWritableTail(int start) {
        return tail.AsSpan(start);
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

}
