using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods.Call;

public class Parameters : NeslParsingTestEnvironment {

    [Fact]
    public void One() {
        CompileSingle(@"
            f32 Foo(f32 value) {
                return value;
            }

            f32 Main(f32 value) {
                return Foo(value);
            }
        ");
    }

    [Fact]
    public void Several() {
        CompileSingle(@"
            f32 Foo(f32 value, f32v4 a) {
                return value;
            }

            f32 Main(f32v4 a, f32 value) {
                return Foo(value, a);
            }
        ");
    }

    [Fact]
    public void NotMaching() {
        CompileSingleThrow(@"
            f32 Foo(f32 value, f32v4 a) {
                return value;
            }

            f32 Main(f32v3 a, f32 value) {
                return Foo(value, a);
            }
        ", CompilationErrorType.MethodWithGivenArgumentsNotFound);
    }

    [Fact]
    public void AdditionalComma() {
        CompileSingleThrow(@"
            f32 Foo(f32 value, f32v4 a) {
                return value;
            }

            f32 Main(f32v3 a, f32 value) {
                return Foo(value, a,);
            }
        ", CompilationErrorType.ExpectedValue);
    }

    [Fact]
    public void MissingComma() {
        CompileSingleThrowAny(@"
            f32 Foo(f32 value, f32v4 a) {
                return value;
            }

            f32 Main(f32v3 a, f32 value) {
                return Foo(value a);
            }
        ");
    }

}
