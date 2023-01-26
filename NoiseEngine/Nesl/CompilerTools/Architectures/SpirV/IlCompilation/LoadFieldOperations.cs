namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class LoadFieldOperations : IlCompilerOperation {

    public LoadFieldOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void SpirVLoadField(SpirVVariable result, SpirVVariable obj, uint index) {
        if (obj.AdditionalData is SpirVVariable[] innerVariables) {
            SpirVId id = Compiler.GetNextId();
            Generator.Emit(
                SpirVOpCode.OpLoad, Compiler.GetSpirVType(result.NeslType).Id, id,
                innerVariables[index].GetAccess(Generator)
            );
            IlCompiler.LoadOperations.SpirVStore(result, id);
            return;
        }

        SpirVType elementType = Compiler.GetSpirVType(result.NeslType);
        SpirVId indexConst = Compiler.GetConst(index);
        SpirVId accessChain = IlCompiler.LoadOperations.GetAccessChainFromIndex(elementType, obj, indexConst);

        SpirVId load = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, elementType.Id, load, accessChain);

        IlCompiler.LoadOperations.SpirVStore(result, load);
    }

    public void LoadField(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable obj = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        uint index = instruction.ReadUInt32();

        SpirVLoadField(result, obj, index);
    }

    public void SetField(Instruction instruction) {
        SpirVVariable obj = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        uint index = instruction.ReadUInt32();
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId load = IlCompiler.LoadOperations.SpirVLoad(value);

        SpirVType elementType = Compiler.GetSpirVType(value.NeslType);
        SpirVId indexConst = Compiler.GetConst(index);
        SpirVId accessChain = IlCompiler.LoadOperations.GetAccessChainFromIndex(elementType, obj, indexConst);

        Generator.Emit(SpirVOpCode.OpStore, accessChain, load);
    }

}
