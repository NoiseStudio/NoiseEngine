using NoiseEngine.Nesl.Emit;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVGenerator {

    public SpirVCompiler Compiler { get; }

    internal SerializationWriter Writer { get; } = new SerializationWriter();

    public SpirVGenerator(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    private static List<byte> StringToLiteralStringBytes(string obj) {
        List<byte> result = new List<byte>(Encoding.UTF8.GetBytes(obj));

        result.Add(0); // null termination

        int count = (sizeof(uint) - result.Count % sizeof(uint)) % sizeof(uint);
        for (int i = 0; i < count; i++)
            result.Add(0);

        return result;
    }

    public void Emit(SpirVOpCode opCode) {
        EmitWorker(opCode, 1);
    }

    public void Emit(SpirVOpCode opCode, uint argument1) {
        EmitWorker(opCode, 2, typeof(uint));
        Writer.WriteUInt32(argument1);
    }

    public void Emit(SpirVOpCode opCode, uint argument1, uint argument2) {
        EmitWorker(opCode, 3, typeof(uint), typeof(uint));
        Writer.WriteUInt32(argument1);
        Writer.WriteUInt32(argument2);
    }

    public void Emit(SpirVOpCode opCode, uint argument1, SpirVId argument2, string argument3) {
        List<byte> bytes = StringToLiteralStringBytes(argument3);
        EmitWorker(opCode, (ushort)(3 + (bytes.Count / 4)), typeof(uint), typeof(SpirVId), typeof(string));

        Writer.WriteUInt32(argument1);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteBytes(bytes);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1) {
        EmitWorker(opCode, 2, typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, uint argument2) {
        EmitWorker(opCode, 3, typeof(SpirVId), typeof(uint));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, uint argument2, SpirVId argument3) {
        EmitWorker(opCode, 4, typeof(SpirVId), typeof(uint), typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2);
        Writer.WriteUInt32(argument3.RawId);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVId argument2) {
        EmitWorker(opCode, 3, typeof(SpirVId), typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2.RawId);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVId argument2, uint argument3) {
        EmitWorker(opCode, 4, typeof(SpirVId), typeof(SpirVId), typeof(uint));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteUInt32(argument3);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVId argument2, uint argument3, SpirVId argument4) {
        EmitWorker(opCode, 5, typeof(SpirVId), typeof(SpirVId), typeof(uint), typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteUInt32(argument3);
        Writer.WriteUInt32(argument4.RawId);
    }

    private void EmitWorker(SpirVOpCode opCode, ushort wordCount, params Type[] expectedTail) {
        AssertTail(opCode, expectedTail);

        Writer.WriteUInt16(wordCount);
        Writer.WriteUInt16((ushort)opCode);
    }

    private void AssertTail(SpirVOpCode opCode, params Type[] expectedTail) {
        if (!opCode.GetCustomAttribute<OpCodeValidationAttribute>().Tail.SequenceEqual(expectedTail))
            throw new InvalidOperationException($"{opCode} does not support given arguments.");
    }

}
