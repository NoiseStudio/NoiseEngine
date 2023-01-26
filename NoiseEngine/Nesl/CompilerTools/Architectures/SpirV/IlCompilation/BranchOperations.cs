using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class BranchOperations : IlCompilerOperation {

    public BranchOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void Call(Instruction instruction) {
        SpirVVariable? result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod);
        NeslMethod method = Assembly.GetMethod(instruction.ReadUInt64());
        Span<SpirVVariable> parameters = instruction.ReadRangeSpirVVariable(IlCompiler, NeslMethod);

        SpirVFunctionIdentifier identifier = new SpirVFunctionIdentifier(method, parameters);
        Span<SpirVId> parameterIds = stackalloc SpirVId[identifier.DynamicParameters];

        int i = 0;
        for (int j = 0; j < parameters.Length; j++) {
            if (identifier.Parameters[j] is null)
                parameterIds[i++] = parameters[j].Id;
        }

        SpirVFunction calledFunction = Compiler.GetSpirVFunction(identifier);

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpFunctionCall, Compiler.GetSpirVType(method.ReturnType).Id, id, calledFunction.Id, parameterIds
        );

        IlCompiler.Function.UsedIOVariables.UnionWith(calledFunction.UsedIOVariables);

        if (result is not null)
            Generator.Emit(SpirVOpCode.OpStore, result.GetAccess(Generator), id);
    }

    public void Return() {
        Generator.Emit(SpirVOpCode.OpReturn);
    }

    public void ReturnValue(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        if (IlCompiler.ExecutionModel.HasValue) {
            switch (IlCompiler.ExecutionModel.Value) {
                case ExecutionModel.Vertex:
                case ExecutionModel.Fragment:
                    IlCompiler.LoadOperations.SpirVStore(
                        IlCompiler.Function.OutputVariable!, IlCompiler.LoadOperations.SpirVLoad(result)
                    );
                    Return();
                    return;
            }
        }

        Generator.Emit(SpirVOpCode.OpReturnValue, IlCompiler.LoadOperations.SpirVLoad(result));
    }

}
