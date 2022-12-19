using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal static class InstructionExtensions {

    public static SpirVVariable? ReadSpirVVariable(
        this ref Instruction instruction, IlCompiler ilCompiler, NeslMethod neslMethod
    ) {
        uint index = instruction.ReadUInt32();
        if (index == uint.MaxValue)
            return null;

        if (index < neslMethod.Type.Fields.Count)
            return ilCompiler.Compiler.GetSpirVVariable(neslMethod.Type.Fields[(int)index]);

        index -= (uint)neslMethod.Type.Fields.Count;
        if (index < ilCompiler.Parameters.Count)
            return ilCompiler.Parameters[(int)index];

        return ilCompiler.DefOperations.Variables[(int)index - ilCompiler.Parameters.Count];
    }

    public static Span<SpirVVariable> ReadRangeSpirVVariable(
        this ref Instruction instruction, IlCompiler ilCompiler, NeslMethod neslMethod
    ) {
        Span<SpirVVariable> span = new SpirVVariable[(int)instruction.ReadUInt32()];
        for (int i = 0; i < span.Length; i++)
            span[i] = ReadSpirVVariable(ref instruction, ilCompiler, neslMethod)!;

        return span;
    }

}
