﻿using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System.Collections.Generic;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class ReadWriteBuffer : IntrinsicsContainer {

    public ReadWriteBuffer(
        SpirVCompiler compiler, NeslMethod neslMethod, SpirVGenerator generator,
        IReadOnlyList<SpirVVariable> parameters
    ) : base(compiler, neslMethod, generator, parameters) {
    }

    public override void Process() {
        switch (NeslMethod.Name) {
            case NeslOperators.IndexerGet:
                IndexerGet();
                break;
            default:
                throw NewUnableFindDefinitionException();
        }
    }

    private SpirVId GetAccessChainFromIndex() {
        SpirVId index = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(Parameters[1].NeslType).Id, index, Parameters[1].Id);

        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(
            StorageClass.Uniform, Compiler.GetSpirVType(Parameters[0].NeslType)
        );

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpAccessChain, pointer.Id, id, Parameters[0].Id,
            new SpirVId[] { Compiler.GetConst(0), index }
        );

        return id;
    }

    private void IndexerGet() {
        SpirVId accessChain = GetAccessChainFromIndex();

        SpirVId result = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(NeslMethod.ReturnType).Id, result, accessChain);
        Generator.Emit(SpirVOpCode.OpReturnValue, result);
    }

}
