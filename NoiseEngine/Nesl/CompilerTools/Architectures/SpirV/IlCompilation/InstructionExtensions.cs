using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal static class InstructionExtensions {

    public static SpirVVariable? ReadSpirVVariable(
        this ref Instruction instruction, IlCompiler ilCompiler, NeslMethod neslMethod
    ) {
        uint index = instruction.ReadUInt32();
        if (index == uint.MaxValue)
            return null;

        SpirVVariable variable;
        if (index < neslMethod.Type.Fields.Count) {
            variable = ilCompiler.Compiler.GetSpirVVariable(neslMethod.Type.Fields[(int)index]);
            ilCompiler.UsedVariables.Add(variable);
            return variable;
        }

        index -= (uint)neslMethod.Type.Fields.Count;
        if (index < ilCompiler.Parameters.Count) {
            variable = ilCompiler.Parameters[(int)index];
            ilCompiler.UsedVariables.Add(variable);
            return variable;
        }

        variable = ilCompiler.DefOperations.Variables[(int)index - ilCompiler.Parameters.Count];
        ilCompiler.UsedVariables.Add(variable);
        return variable;
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
