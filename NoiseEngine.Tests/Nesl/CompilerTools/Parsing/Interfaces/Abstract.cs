using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Abstract : NeslParsingTestEnvironment {

    [Fact]
    public void MethodWithoutParameters() {
        CompileSingle(@"
            interface HelloWorld {
                f32 Get();
            }
        ");
    }

    [Fact]
    public void MethodWithParameters() {
        CompileSingle(@"
            interface HelloWorld {
                f32 Sum(f32 a, f32 b);
            }
        ");
    }

    [Fact]
    public void InheritanceWithAbstractImplementation() {
        CompileSingle(@"
            interface IBar {
                f32 Sum(f32 a, f32 b);
            }

            interface Foo : IBar {
                public f32 Sum(f32 a, f32 b) {
                    return a + b;
                }
            }
        ");
    }

    [Fact]
    public void InheritanceWithoutAbstractImplementation() {
        CompileSingleThrow(@"
            interface IBar {
                f32 Sum(f32 a, f32 b);
            }

            interface Foo : IBar {
            }
        ", CompilationErrorType.AbstractMethodNotImplemented);
    }

    [Fact]
    public void InheritanceWithMixedAbstractImplementation() {
        CompileSingleThrow(@"
            interface IBar {
                f32 Sum(f32 a, f32 b);
            }

            interface IBaz<T> {
                static T Huh(T a);
            }

            interface Foo : IBar, IBaz<Foo> {
                public f32 Sum(f32 a, f32 b) {
                    return a + b;
                }
            }
        ", CompilationErrorType.AbstractMethodNotImplemented);
    }

}
