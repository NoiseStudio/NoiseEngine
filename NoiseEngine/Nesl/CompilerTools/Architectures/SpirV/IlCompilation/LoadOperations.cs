namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class LoadOperations : IlCompilerOperation {

    public LoadOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public SpirVId SpirVLoad(SpirVVariable variable) {
        SpirVId id = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpLoad, Compiler.GetSpirVType(variable.NeslType).Id, id, variable.GetAccess(Generator)
        );
        return id;
    }

    public void SpirVStore(SpirVVariable variable, SpirVId constId) {
        Generator.Emit(SpirVOpCode.OpStore, variable.GetAccess(Generator), constId);
    }

    public void Load(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(result.NeslType).Id, id, value.GetAccess(Generator));
        SpirVStore(result, id);
    }

    public void LoadUInt32(Instruction instruction) {
        SpirVStore(instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!, Compiler.GetConst(instruction.ReadUInt32()));
    }

    public void LoadFloat32(Instruction instruction) {
        SpirVStore(instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!, Compiler.GetConst(instruction.ReadFloat32()));
    }

}
