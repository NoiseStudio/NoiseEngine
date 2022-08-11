namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVType {

    public SpirVCompiler Compiler { get; }
    public NeslType? NeslType { get; }

    public SpirVId Id { get; }

    public SpirVType(SpirVCompiler compiler, SpirVId id) {
        Compiler = compiler;
        Id = id;
    }

    public SpirVType(SpirVCompiler compiler, NeslType neslType) {
        Compiler = compiler;
        NeslType = neslType;

        Id = Compiler.GetNextId();
    }

}
