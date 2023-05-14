using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods.Call;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void Next() {
        CompileSingle(@"
            struct Bar {}

            Bar Main() {
                return Foo();
            }

            Bar Foo() {
                return new Bar();
            }
        ");
    }

    [Fact]
    public void Nested() {
        CompileSingle(@"
            struct Bar {

                static Bar Foo() {
                    return new Bar();
                }

            }

            Bar Main() {
                return Bar.Foo();
            }
        ");
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrow(@"
            struct Bar {}

            Bar Main() {
                return Foo674574523424();
            }
        ", CompilationErrorType.MethodNotFound);
    }

}
