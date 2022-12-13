namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal abstract class IlCompilerOperation {

    public IlCompiler IlCompiler { get; }

    public SpirVCompiler Compiler => IlCompiler.Compiler;
    public SpirVGenerator Generator => IlCompiler.Generator;
    public NeslMethod NeslMethod => IlCompiler.NeslMethod;
    public NeslAssembly Assembly => NeslMethod.Assembly;

    protected IlCompilerOperation(IlCompiler ilCompiler) {
        IlCompiler = ilCompiler;
    }

}
