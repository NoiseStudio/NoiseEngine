using NoiseEngine.Collections;
using NoiseEngine.Nesl.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Emit;

public class IlGenerator : IlContainer {

    private readonly List<(OpCode opCode, uint tailIndex)> rawInstructions = new List<(OpCode, uint)>();
    private readonly FastList<byte> tail = new FastList<byte>();

    protected override IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions => rawInstructions;

    internal IlGenerator() {
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>
    public void Emit(OpCode opCode) {
        rawInstructions.Add((opCode, (uint)tail.Count));
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, byte argument1) {
        EmitWorker(opCode, typeof(byte));
        tail.Add(argument1);
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

    private void EmitWorker(OpCode opCode, params Type[] expectedTail) {
        AssertTail(opCode, expectedTail);
        Emit(opCode);
    }

    private void AssertTail(OpCode opCode, params Type[] expectedTail) {
        if (!opCode.GetAttribute<OpCodeValidationAttribute>().Tail.SequenceEqual(expectedTail))
            throw new InvalidOperationException($"{opCode} does not support given arguments.");
    }

}
