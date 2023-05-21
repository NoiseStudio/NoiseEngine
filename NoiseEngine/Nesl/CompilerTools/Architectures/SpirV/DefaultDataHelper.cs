using NoiseEngine.Nesl.Default;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal static class DefaultDataHelper {

    public static void Store(
        SpirVCompiler compiler, SpirVVariable variable, NeslField neslField, SpirVGenerator generator
    ) {
        if (neslField.FieldType.FullNameWithAssembly != Buffers.RwBufferName)
            throw new NotImplementedException();

        SpirVId constId = compiler.Consts.GetConsts(variable.NeslType, neslField.DefaultData!);
        generator.Emit(SpirVOpCode.OpStore, variable.GetAccess(generator), constId);
    }

}
