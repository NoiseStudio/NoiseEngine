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
            case $"{DefaultAssembly}.{nameof(Compute)}":
                 new Compute(generator, function, parameters).Process();
                 break;
            case $"{DefaultAssembly}.{nameof(Vertex)}":
                new Vertex(generator, function, parameters).Process();
                break;
            default:
                throw new InvalidOperationException($"Unable to find given {nameof(IntrinsicAttribute)} definition.");
        }
    }

}
