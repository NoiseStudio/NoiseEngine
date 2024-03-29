﻿using NoiseEngine.Rendering.PushConstants;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResultBuilder {

    public byte[]? Code { get; set; }
    public Dictionary<NeslField, uint> Bindings { get; } = new Dictionary<NeslField, uint>();
    public Dictionary<NeslMethod, VertexInputDescription> VertexInputDesciptions { get; } =
        new Dictionary<NeslMethod, VertexInputDescription>();
    public List<PushConstantDescriptor> PushConstantDescriptors { get; } = new List<PushConstantDescriptor>();

    public SpirVCompilationResult Build() {
        return new SpirVCompilationResult(this);
    }

}
