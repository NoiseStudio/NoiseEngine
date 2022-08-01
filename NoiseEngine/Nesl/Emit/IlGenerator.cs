using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Emit;

public class IlGenerator : IlContainer {

    private readonly NeslAssemblyBuilder assembly;
    private readonly List<(OpCode opCode, uint tailIndex)> rawInstructions = new List<(OpCode, uint)>();
    private readonly FastList<byte> tail = new FastList<byte>();

    protected override IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions => rawInstructions;

    internal IlGenerator(NeslAssemblyBuilder assemblyBuilder) {
        assembly = assemblyBuilder;
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

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, ulong argument1) {
        EmitWorker(opCode, typeof(ulong));

        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, argument1);
        tail.AddRange(bytes);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, float argument1) {
        EmitWorker(opCode, typeof(float));

        Span<byte> bytes = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(bytes, argument1);
        tail.AddRange(bytes);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, NeslMethod argument1) {
        EmitWorker(opCode, typeof(NeslMethod));

        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, assembly.GetLocalMethodId(argument1));
        tail.AddRange(bytes);
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

    private void EmitWorker(OpCode opCode, params Type[] expectedTail) {
        AssertTail(opCode, expectedTail);
        Emit(opCode);
    }

    private void AssertTail(OpCode opCode, params Type[] expectedTail) {
        if (!opCode.GetCustomAttribute<OpCodeValidationAttribute>().Tail.SequenceEqual(expectedTail))
            throw new InvalidOperationException($"{opCode} does not support given arguments.");
    }

}
