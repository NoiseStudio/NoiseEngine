using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResult {

    private byte[]? code;

    internal SpirVCompilationResult(byte[]? code) {
        this.code = code;
    }

    public byte[] GetCode() {
        return code ?? throw new NullReferenceException();
    }

}
