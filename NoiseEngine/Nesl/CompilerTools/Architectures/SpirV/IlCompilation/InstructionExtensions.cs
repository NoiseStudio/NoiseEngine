namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal static class InstructionExtensions {

    public static SpirVVariable ReadSpirVVariable(
        this ref Instruction instruction, IlCompiler ilCompiler, NeslMethod neslMethod
    ) {
        uint index = instruction.ReadUInt32();

        if (index < neslMethod.Type.Fields.Count)
            return ilCompiler.Compiler.GetSpirVVariable(neslMethod.Type.Fields[(int)index]);

        return ilCompiler.DefOperations.Variables[(int)index - neslMethod.Type.Fields.Count];
    }

}
