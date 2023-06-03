using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class GenericConstraints : NeslParsingTestEnvironment {

    [Fact]
    public void Implement() {
        CompileSingle(@"
            interface Mock {}
            struct Foo : Mock {}

            struct Wrapper<T> {
                public T Value;

                public T Unwrap() where T : Mock {
                    return Value;
                }
            }

            Foo Run() {
                return new Wrapper<Foo>() {
                    Value = new Foo()
                }.Unwrap();
            }
        ");
    }

    [Fact]
    public void NotImplement() {
        CompileSingleThrow(@"
            interface Mock {}

            struct Wrapper<T> {
                public T Value;

                public T Unwrap() where T : Mock {
                    return Value;
                }
            }

            f32 Run() {
                return new Wrapper<f32>() {
                    Value = 1.0
                }.Unwrap();
            }
        ", CompilationErrorType.MethodNotFound);
    }

}
