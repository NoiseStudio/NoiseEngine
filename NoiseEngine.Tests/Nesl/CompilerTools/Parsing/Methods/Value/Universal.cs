using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods.Value;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void Variable() {
        CompileSingle(@"
            f32 Main(f32 value) {
                return value;
            }
        ");
    }

    [Fact]
    public void VariableNested() {
        CompileSingle(@"
            struct A {
                f32 Value;
            }

            struct B {
                A A;
            }

            struct C {
                B B;
            }

            f32 Main(C c) {
                return c.B.A.Value;
            }
        ");
    }

}
