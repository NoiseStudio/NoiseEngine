namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class LoadElementOperations : IlCompilerOperation {

    public LoadElementOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void LoadElement(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable buffer = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable index = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVType elementType = Compiler.GetSpirVType(result.NeslType);
        SpirVId accessChain = GetAccessChainFromIndex(elementType, buffer, index);

        SpirVId load = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, elementType.Id, load, accessChain);

        IlCompiler.LoadOperations.SpirVStore(result, load);
    }

    public void SetElement(Instruction instruction) {
        SpirVVariable buffer = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable index = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId load = IlCompiler.LoadOperations.SpirVLoad(value);

        SpirVType elementType = Compiler.GetSpirVType(value.NeslType);
        SpirVId accessChain = GetAccessChainFromIndex(elementType, buffer, index);

        Generator.Emit(SpirVOpCode.OpStore, accessChain, load);
    }

    private SpirVId GetAccessChainFromIndex(SpirVType elementType, SpirVVariable buffer, SpirVVariable index) {
        SpirVId indexId = IlCompiler.LoadOperations.SpirVLoad(index);
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(buffer.StorageClass, elementType);

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpAccessChain, pointer.Id, id, buffer.Id,
            stackalloc SpirVId[] { Compiler.GetConst(0), indexId }
        );

        return id;
    }

}
