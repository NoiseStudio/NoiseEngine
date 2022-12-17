using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class BranchOperations : IlCompilerOperation {

    public BranchOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void Call(Instruction instruction) {
        SpirVVariable? result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod);
        NeslMethod method = Assembly.GetMethod(instruction.ReadUInt64());
        Span<SpirVVariable> parameters = instruction.ReadRangeSpirVVariable(IlCompiler, NeslMethod);

        Span<SpirVId> parameterIds = stackalloc SpirVId[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
            parameterIds[i] = parameters[i].Id;

        Generator.Emit(
            SpirVOpCode.OpFunctionCall, Compiler.GetSpirVType(method.ReturnType).Id, result?.Id ?? Compiler.GetNextId(),
            Compiler.GetSpirVFunction(method).Id, parameterIds
        );
    }

    public void Return() {
        Generator.Emit(SpirVOpCode.OpReturn);
    }

}
