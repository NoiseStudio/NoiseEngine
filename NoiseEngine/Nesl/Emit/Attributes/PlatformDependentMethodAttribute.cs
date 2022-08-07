using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class PlatformDependentMethodAttribute {

    internal IReadOnlyList<byte>? CilDefinition { get; }
    internal IReadOnlyList<byte>? SpirVDefinition { get; }

    public PlatformDependentMethodAttribute(byte[]? cilDefiniton, byte[]? spirVDefinition) {
        CilDefinition = cilDefiniton;
        SpirVDefinition = spirVDefinition;
    }

}
