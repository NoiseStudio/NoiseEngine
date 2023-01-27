using NoiseEngine.Rendering.PushConstants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResult {

    private byte[]? code;

    public IReadOnlyList<(NeslField, uint)> Bindings { get; }
    public IReadOnlyDictionary<NeslMethod, VertexInputDescription> VertexInputDesciptions { get; }
    public IReadOnlyList<PushConstantDescriptor> PushConstantDescriptors { get; }

    internal SpirVCompilationResult(SpirVCompilationResultBuilder builder) {
        code = builder.Code;
        Bindings = builder.Bindings.Keys.Select(x => (x, builder.Bindings[x])).OrderBy(x => x.Item2).ToArray();
        VertexInputDesciptions = builder.VertexInputDesciptions;
        PushConstantDescriptors = builder.PushConstantDescriptors;
    }

    public byte[] GetCode() {
        return code ?? throw new NullReferenceException();
    }

}
