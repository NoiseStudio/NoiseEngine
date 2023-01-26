using NoiseEngine.Rendering.Vulkan;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResultBuilder {

    public byte[]? Code { get; set; }
    public Dictionary<NeslField, uint> Bindings { get; } = new Dictionary<NeslField, uint>();
    public Dictionary<NeslMethod, VertexInputDescription> VertexInputDesciptions { get; } =
        new Dictionary<NeslMethod, VertexInputDescription>();

    public SpirVCompilationResult Build() {
        return new SpirVCompilationResult(Code, Bindings, VertexInputDesciptions);
    }

}
