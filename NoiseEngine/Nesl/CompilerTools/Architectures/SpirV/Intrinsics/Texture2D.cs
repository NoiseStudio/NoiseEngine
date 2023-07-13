using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class Texture2D : IntrinsicsContainer {

    public Texture2D(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) : base(generator, function, parameters) {
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

    private void IndexerGet() {
        SpirVVariable texture = Parameters[0];
        SpirVVariable index = Parameters[1];

        SpirVType elementType = Compiler.GetSpirVType(NeslMethod.ReturnType);
        SpirVId sampledId = LoadOperations.SpirVLoad(Generator, texture);
        SpirVId indexId = LoadOperations.SpirVLoad(Generator, index);

        SpirVId resultId = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpImageSampleImplicitLod, elementType.Id, resultId, sampledId, indexId);

        Generator.Emit(SpirVOpCode.OpReturnValue, resultId);
    }

}
