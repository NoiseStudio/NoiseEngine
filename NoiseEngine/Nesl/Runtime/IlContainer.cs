using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Runtime;

public abstract class IlContainer {

    protected abstract IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions { get; }

    internal IEnumerable<Instruction> Instructions => GetInstructions();

    internal abstract ReadOnlySpan<byte> GetTail(int start);

    private IEnumerable<Instruction> GetInstructions() {
        foreach ((OpCode opCode, uint tailIndex) in RawInstructions)
            yield return new Instruction(opCode, tailIndex, this);
    }

}
