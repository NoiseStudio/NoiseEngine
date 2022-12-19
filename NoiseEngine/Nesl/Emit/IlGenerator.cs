using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class IlGenerator : IlContainer {

    private readonly NeslAssemblyBuilder assembly;
    private readonly NeslMethodBuilder method;
    private readonly List<(OpCode opCode, uint tailIndex)> rawInstructions = new List<(OpCode, uint)>();
    private readonly SerializationWriter tail = new SerializationWriter();

    protected override IEnumerable<(OpCode opCode, uint tailIndex)> RawInstructions => rawInstructions;

    internal IlGenerator(NeslAssemblyBuilder assembly, NeslMethodBuilder method) {
        this.assembly = assembly;
        this.method = method;
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
    public void Emit(OpCode opCode, uint argument1) {
        EmitWorker(opCode, typeof(uint));
        tail.WriteUInt32(argument1);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    public void Emit(OpCode opCode, uint argument1, uint argument2) {
        EmitWorker(opCode, typeof(uint), typeof(uint));
        tail.WriteUInt32(argument1);
        tail.WriteUInt32(argument2);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    /// <param name="argument3">Third argument.</param>
    public void Emit(OpCode opCode, uint argument1, uint argument2, uint argument3) {
        EmitWorker(opCode, typeof(uint), typeof(uint), typeof(uint));
        tail.WriteUInt32(argument1);
        tail.WriteUInt32(argument2);
        tail.WriteUInt32(argument3);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    public void Emit(OpCode opCode, uint argument1, float argument2) {
        EmitWorker(opCode, typeof(uint), typeof(float));
        tail.WriteUInt32(argument1);
        tail.WriteFloat32(argument2);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    /// <param name="argument3">Third argument.</param>
    public void Emit(OpCode opCode, uint argument1, NeslMethod argument2, ReadOnlySpan<uint> argument3) {
        EmitWorker(opCode, typeof(uint), typeof(NeslMethod), typeof(uint[]));
        tail.WriteUInt32(argument1);
        tail.WriteUInt64(assembly.GetLocalMethodId(argument2));

        tail.WriteUInt32((uint)argument3.Length);
        foreach (uint element in argument3)
            tail.WriteUInt32(element);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, NeslType argument1) {
        EmitWorker(opCode, typeof(NeslType));
        tail.WriteUInt64(assembly.GetLocalTypeId(argument1));
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

    private void EmitWorker(OpCode opCode, params Type[] expectedTail) {
        OpCodeValidationAttribute.AssertTail(opCode, expectedTail);
        Emit(opCode);
    }

}
