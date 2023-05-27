using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class RwBuffer : IntrinsicsContainer {

    public RwBuffer(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) : base(generator, function, parameters) {
    }

    public override void Process() {
        switch (NeslMethod.Name) {
            case NeslOperators.IndexerSet:
                IndexerSet();
                break;
            case NeslOperators.IndexerGet:
                IndexerGet();
                break;
            default:
                throw NewUnableFindDefinitionException();
        }
    }

    private void IndexerSet() {
        Debug.Assert(NeslMethod.ReturnType is null);

        SpirVVariable instance = Parameters[0];
        SpirVVariable index = Parameters[1];
        SpirVVariable value = Parameters[2];

        SpirVId load = LoadOperations.SpirVLoad(Generator, value);

        SpirVType elementType = Compiler.GetSpirVType(value.NeslType);
        SpirVId accessChain = GetAccessChainFromIndex(elementType, instance, index);

        Generator.Emit(SpirVOpCode.OpStore, accessChain, load);
        Generator.Emit(SpirVOpCode.OpReturn);
    }

    private void IndexerGet() {
        SpirVVariable result = Function.OutputVariable!;
        SpirVVariable buffer = Parameters[0];
        SpirVVariable index = Parameters[1];

        SpirVType elementType = Compiler.GetSpirVType(result.NeslType);
        SpirVId accessChain = GetAccessChainFromIndex(elementType, buffer, index);

        SpirVId load = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, elementType.Id, load, accessChain);

        LoadOperations.SpirVStore(Generator, result, load);
        Generator.Emit(SpirVOpCode.OpReturn);
    }

    private SpirVId GetAccessChainFromIndex(SpirVType elementType, SpirVVariable buffer, SpirVVariable index) {
        SpirVId indexId = LoadOperations.SpirVLoad(Generator, index);
        return LoadOperations.GetAccessChainFromIndex(Generator, elementType, buffer, indexId);
    }

}
