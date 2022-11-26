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
    public void Emit(OpCode opCode, byte argument1) {
        EmitWorker(opCode, typeof(byte));
        tail.WriteUInt8(argument1);
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
    public void Emit(OpCode opCode, uint argument1, ulong argument2) {
        EmitWorker(opCode, typeof(uint), typeof(ulong));
        tail.WriteUInt32(argument1);
        tail.WriteUInt64(argument2);
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
    public void Emit(OpCode opCode, uint argument1, NeslField argument2) {
        EmitWorker(opCode, typeof(uint), typeof(NeslField));
        tail.WriteUInt32(argument1);
        tail.WriteUInt64(method.Type.GetLocalFieldId(argument2));
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, ulong argument1) {
        EmitWorker(opCode, typeof(ulong));
        tail.WriteUInt64(argument1);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, float argument1) {
        EmitWorker(opCode, typeof(float));
        tail.WriteFloat32(argument1);
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

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    public void Emit(OpCode opCode, NeslType argument1, uint argument2) {
        EmitWorker(opCode, typeof(NeslType), typeof(uint));
        tail.WriteUInt64(assembly.GetLocalTypeId(argument1));
        tail.WriteUInt32(argument2);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, NeslField argument1) {
        EmitWorker(opCode, typeof(NeslField));
        tail.WriteUInt64(method.Type.GetLocalFieldId(argument1));
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    /// <param name="argument2">Second argument.</param>
    public void Emit(OpCode opCode, NeslField argument1, uint argument2) {
        EmitWorker(opCode, typeof(NeslField), typeof(uint));
        tail.WriteUInt64(method.Type.GetLocalFieldId(argument1));
        tail.WriteUInt32(argument2);
    }

    /// <summary>
    /// Puts <paramref name="opCode"/> with given arguments to stream of instructions.
    /// </summary>
    /// <param name="opCode">The NESIL instruction <see cref="OpCode"/>.</param>>
    /// <param name="argument1">First argument.</param>
    public void Emit(OpCode opCode, NeslMethod argument1) {
        EmitWorker(opCode, typeof(NeslMethod));
        tail.WriteUInt64(assembly.GetLocalMethodId(argument1));
    }

    internal override ReadOnlySpan<byte> GetTail(int start) {
        return tail.AsSpan(start);
    }

    private void EmitWorker(OpCode opCode, params Type[] expectedTail) {
        OpCodeValidationAttribute.AssertTail(opCode, expectedTail);
        Emit(opCode);
    }

}
