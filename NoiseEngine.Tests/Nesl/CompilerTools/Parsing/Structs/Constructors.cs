using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Constructors : NeslParsingTestEnvironment {

    [Fact]
    public void WithoutParameters() {
        CompileSingle(@"
            struct Foo {

                public u32 X;

                public Foo() {
                    X = 42;
                }

            }
        ");
    }

    [Fact]
    public void WithParameters() {
        CompileSingle(@"
            struct Foo {

                public f32 X;

                public Foo(f32 x) {
                    X = x;
                }

            }
        ");
    }

}
