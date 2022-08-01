using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Attributes;

public class PlatformDependentMethodNeslAttribute : NeslAttribute {

    internal IReadOnlyList<byte>? CilDefinition { get; }
    internal IReadOnlyList<byte>? SpirVDefinition { get; }

    public PlatformDependentMethodNeslAttribute(byte[]? cilDefiniton, byte[]? spirVDefinition) {
        CilDefinition = cilDefiniton;
        SpirVDefinition = spirVDefinition;
    }

}
