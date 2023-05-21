using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class Assignment : NeslParsingTestEnvironment {

    [Fact]
    public void CompactValueFromVariable() {
        CompileSingle(@"
            void Main(f32 a, f32v3 b) {
                a = b.Z;
            }
        ");
    }

    [Fact]
    public void CompactValueFromVariableConstructor() {
        CompileSingle(@"
            struct Foo {
                Foo(f32 a, f32v3 b) {
                    a = b.Z;
                }
            }
        ");
    }

    [Fact]
    public void CompactValueFromField() {
        CompileSingle(@"
            struct Foo {

                f32 a;
                f32v4 b;

                void Main(f32 a) {
                    a = b.Y;
                }

            }
        ");
    }

}
