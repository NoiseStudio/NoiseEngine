using System.Collections.Generic;

namespace NoiseEngine.Nesl.Runtime.Attributes;

public class PlatformDependentNeslAttribute : NeslAttribute {

    internal IReadOnlyList<byte>? CilDefinition { get; }
    internal IReadOnlyList<byte>? SpirVDefinition { get; }

    public PlatformDependentNeslAttribute(byte[]? cilDefiniton, byte[]? spirVDefinition) {
        CilDefinition = cilDefiniton;
        SpirVDefinition = spirVDefinition;
    }

}
