using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResult {

    private byte[]? code;

    public IReadOnlyList<(NeslField, uint)> Bindings { get; }
    public IReadOnlyDictionary<NeslMethod, VertexInputDescription> VertexInputDesciptions { get; }

    internal SpirVCompilationResult(
        byte[]? code, IReadOnlyDictionary<NeslField, uint> bindings,
        IReadOnlyDictionary<NeslMethod, VertexInputDescription> vertexInputAttributeDesciptions
    ) {
        this.code = code;
        Bindings = bindings.Keys.Select(x => (x, bindings[x])).OrderBy(x => x.Item2).ToArray();
        VertexInputDesciptions = vertexInputAttributeDesciptions;
    }

    public byte[] GetCode() {
        return code ?? throw new NullReferenceException();
    }

}
