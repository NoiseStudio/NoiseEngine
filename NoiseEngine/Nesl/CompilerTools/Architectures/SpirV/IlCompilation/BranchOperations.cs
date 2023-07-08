using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;
using System.Diagnostics;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class BranchOperations : IlCompilerOperation {

    public BranchOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void Call(Instruction instruction) {
        SpirVVariable? result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod);
        NeslMethod method = Assembly.GetMethod(instruction.ReadUInt64());
        Span<SpirVVariable> parameters = instruction.ReadRangeSpirVVariable(IlCompiler, NeslMethod);

        SpirVFunctionIdentifier identifier = new SpirVFunctionIdentifier(method, parameters);
        Span<SpirVId> parameterIds = stackalloc SpirVId[identifier.DynamicParameters.Count];

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
        SpirVVariable? result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod);
        if (result is null)
            throw new UnreachableException("Return variable cannot be uint.MaxValue.");

        if (IlCompiler.ExecutionModel.HasValue) {
            switch (IlCompiler.ExecutionModel.Value) {
                case ExecutionModel.Vertex:
                    uint index = 0;
                    foreach (NeslField field in result.NeslType.Fields) {
                        switch (field.Name) {
                            case "Position":
                                SpirVVariable position =
                                    Compiler.BuiltInVariables.GetBuiltIn(field.FieldType, StorageClass.Output, 0);
                                IlCompiler.Function.UsedIOVariables.Add(position);
                                IlCompiler.LoadFieldOperations.SpirVLoadField(position, result, index);
                                break;
                        }
                        index++;
                    }

                    ReturnValueStore(result);
                    return;
                case ExecutionModel.Fragment:
                    ReturnValueStore(result);
                    return;
            }
        }

        Generator.Emit(SpirVOpCode.OpReturnValue, IlCompiler.LoadOperations.SpirVLoad(result));
    }

    private void ReturnValueStore(SpirVVariable result) {
        IlCompiler.LoadOperations.SpirVStore(
            IlCompiler.Function.OutputVariable!, IlCompiler.LoadOperations.SpirVLoad(result)
        );
        Generator.Emit(SpirVOpCode.OpReturn);
    }

}
