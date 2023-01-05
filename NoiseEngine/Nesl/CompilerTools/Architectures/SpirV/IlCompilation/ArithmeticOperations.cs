namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class ArithmeticOperations : IlCompilerOperation {

    public ArithmeticOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void Negate(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId resultId = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpFNegate, Compiler.GetSpirVType(result.NeslType).Id, resultId,
            IlCompiler.LoadOperations.SpirVLoad(value)
        );

        IlCompiler.LoadOperations.SpirVStore(result, resultId);
    }

}
