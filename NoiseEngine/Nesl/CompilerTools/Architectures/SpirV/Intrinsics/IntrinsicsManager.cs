using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal static class IntrinsicsManager {

    private const string DefaultAssembly = "System";

    public static void Process(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) {
        if (function.NeslMethod.Assembly.Name != DefaultAssembly) {
            throw new InvalidOperationException(
                $"{nameof(IntrinsicAttribute)} can only be used in {DefaultAssembly} assembly."
            );
        }

        switch (function.NeslMethod.Type.FullName) {
            case $"{DefaultAssembly}.{nameof(ComputeUtils)}":
                 new ComputeUtils(generator, function, parameters).Process();
                 break;
            case $"{DefaultAssembly}.{nameof(VertexUtils)}":
                new VertexUtils(generator, function, parameters).Process();
                break;
            case $"{DefaultAssembly}.RwBuffer`1":
                new RwBuffer(generator, function, parameters).Process();
                break;
            case $"{DefaultAssembly}.Texture2D`1":
                new Texture2D(generator, function, parameters).Process();
                break;
            default:
                throw new InvalidOperationException($"Unable to find given {nameof(IntrinsicAttribute)} definition.");
        }
    }

}
