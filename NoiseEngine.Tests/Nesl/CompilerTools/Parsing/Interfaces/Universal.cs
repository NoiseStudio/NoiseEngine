using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void NotAllowedDefineFields() {
        CompileSingleThrow(@"
            interface HelloWorld {
                f32 a;
            }
        ", CompilationErrorType.FieldInInterfaceNotAllowed);
    }

    [Fact]
    public void NotAllowedDefineConstructor() {
        CompileSingleThrow(@"
            interface HelloWorld {
                public HelloWorld() {}
            }
        ", CompilationErrorType.ConstructorInInterfaceNotAllowed);
    }

    [Fact]
    public void DefineMethod() {
        CompileSingle(@"
            public interface Foo {

                f32 Get();

                public f32 GetContainer() {
                    return Get();
                }

            }
        ");
    }

}
