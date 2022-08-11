namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompilationResultBuilder {

    public byte[]? Code { get; set; }

    public SpirVCompilationResult Build() {
        return new SpirVCompilationResult(Code);
    }

}
