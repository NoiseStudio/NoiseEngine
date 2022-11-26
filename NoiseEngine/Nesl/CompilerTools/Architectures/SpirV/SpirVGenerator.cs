using NoiseEngine.Nesl.Emit;
using NoiseEngine.Serialization;
using System;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVGenerator {

    public SpirVCompiler Compiler { get; }

    internal SerializationWriter Writer { get; } = new SerializationWriter();

    public SpirVGenerator(SpirVCompiler compiler) {
        Compiler = compiler;
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

    public void Emit(SpirVOpCode opCode, uint argument1, SpirVId argument2, SpirVLiteral argument3) {
        EmitWorker(opCode, (ushort)(3 + argument3.WordCount), typeof(uint), typeof(SpirVId), typeof(SpirVLiteral));

        Writer.WriteUInt32(argument1);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteBytes(argument3.Bytes.ToArray());
    }

    public void Emit(
        SpirVOpCode opCode, uint argument1, SpirVId argument2, SpirVLiteral argument3, SpirVId[] argument4
    ) {
        EmitWorker(
            opCode, (ushort)(3 + argument3.WordCount + argument4.Length),
            typeof(uint), typeof(SpirVId), typeof(SpirVLiteral), typeof(SpirVId[])
        );

        Writer.WriteUInt32(argument1);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteBytes(argument3.Bytes.ToArray());

        foreach (SpirVId id in argument4)
            Writer.WriteUInt32(id.RawId);
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

    public void Emit(SpirVOpCode opCode, SpirVId argument1, uint argument2, SpirVLiteral argument3) {
        EmitWorker(opCode, (ushort)(3 + argument3.WordCount), typeof(SpirVId), typeof(uint), typeof(SpirVLiteral));

        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2);
        Writer.WriteBytes(argument3.Bytes.ToArray());
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, float argument2) {
        EmitWorker(opCode, 3, typeof(SpirVId), typeof(float));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteFloat32(argument2);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, uint argument2, SpirVId argument3) {
        EmitWorker(opCode, 4, typeof(SpirVId), typeof(uint), typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2);
        Writer.WriteUInt32(argument3.RawId);
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVLiteral argument2) {
        EmitWorker(opCode, (ushort)(2 + argument2.WordCount), typeof(SpirVId), typeof(SpirVLiteral));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteBytes(argument2.Bytes.ToArray());
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

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVId argument2, SpirVLiteral argument3) {
        EmitWorker(opCode, (ushort)(3 + argument3.WordCount), typeof(SpirVId), typeof(SpirVId), typeof(SpirVLiteral));

        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteBytes(argument3.Bytes.ToArray());
    }

    public void Emit(SpirVOpCode opCode, SpirVId argument1, SpirVId argument2, uint argument3, SpirVId argument4) {
        EmitWorker(opCode, 5, typeof(SpirVId), typeof(SpirVId), typeof(uint), typeof(SpirVId));
        Writer.WriteUInt32(argument1.RawId);
        Writer.WriteUInt32(argument2.RawId);
        Writer.WriteUInt32(argument3);
        Writer.WriteUInt32(argument4.RawId);
    }

    private void EmitWorker(SpirVOpCode opCode, ushort wordCount, params Type[] expectedTail) {
        OpCodeValidationAttribute.AssertTail(opCode, expectedTail);

        Writer.WriteUInt16((ushort)opCode);
        Writer.WriteUInt16(wordCount);
    }

}
