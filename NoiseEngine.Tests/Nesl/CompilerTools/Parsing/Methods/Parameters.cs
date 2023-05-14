using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class Parameters : NeslParsingTestEnvironment {

    [Fact]
    public void DefineWithoutParameters() {
        CompileSingle(@"
            void Main() {}
        ");
    }

    [Fact]
    public void DefineWithOneParameter() {
        CompileSingle(@"
            void Main(f32v3 a) {}
        ");
    }

    [Fact]
    public void DefineWithSeveralParameters() {
        CompileSingle(@"
            void Main(f32 b, f32v4 e) {}
        ");
    }

    [Fact]
    public void DefineMissingComma() {
        CompileSingleThrowAny(@"
            void Main(f32 b f32v4 e) {}
        ");
    }

    [Fact]
    public void DefineAditionalComma() {
        CompileSingleThrow(@"
            void Main(f32 b, f32v4 e,) {}
        ", CompilationErrorType.UnexpectedExpression);
    }

    [Fact]
    public void DefineAditionalCommaInMiddle() {
        CompileSingleThrow(@"
            void Main(f32 b,, f32v4 e) {}
        ", CompilationErrorType.UnexpectedExpression);
    }

}
