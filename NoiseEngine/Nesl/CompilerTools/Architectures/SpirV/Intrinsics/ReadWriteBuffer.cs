using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class ReadWriteBuffer : IntrinsicsContainer {

    public ReadWriteBuffer(
        SpirVCompiler compiler, NeslMethod neslMethod, SpirVGenerator generator, IReadOnlyList<SpirVVariable> parameters
    ) : base(compiler, neslMethod, generator, parameters) {
    }

    public override void Process() {
        switch (NeslMethod.Name) {
            case NeslOperators.IndexerGet:
                IndexerGet();
                break;
            case NeslOperators.IndexerSet:
                IndexerSet();
                break;
            default:
                throw NewUnableFindDefinitionException();
        }
    }

    private SpirVId GetAccessChainFromIndex(SpirVType elementType) {
        SpirVId index = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(Parameters[1].NeslType).Id, index, Parameters[1].Id);

        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(Parameters[0].StorageClass, elementType);

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpAccessChain, pointer.Id, id, Parameters[0].Id,
            new SpirVId[] { Compiler.GetConst(0), index }
        );

        return id;
    }

    private void IndexerGet() {
        SpirVType elementType = Compiler.GetSpirVType(NeslMethod.ReturnType);
        SpirVId accessChain = GetAccessChainFromIndex(elementType);

        SpirVId result = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, elementType.Id, result, accessChain);
        Generator.Emit(SpirVOpCode.OpReturnValue, result);
    }

    private void IndexerSet() {
        SpirVType elementType = Compiler.GetSpirVType(Parameters[2].NeslType);

        SpirVId value = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, elementType.Id, value, Parameters[2].Id);

        SpirVId accessChain = GetAccessChainFromIndex(elementType);
        Generator.Emit(SpirVOpCode.OpStore, accessChain, value);

        Generator.Emit(SpirVOpCode.OpReturn);
    }

}
