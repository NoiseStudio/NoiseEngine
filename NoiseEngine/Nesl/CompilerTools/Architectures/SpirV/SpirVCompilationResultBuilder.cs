using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResultBuilder {

    public byte[]? Code { get; set; }
    public Dictionary<NeslField, uint> Bindings { get; } = new Dictionary<NeslField, uint>();

    public SpirVCompilationResult Build() {
        return new SpirVCompilationResult(Code, Bindings);
    }

}
