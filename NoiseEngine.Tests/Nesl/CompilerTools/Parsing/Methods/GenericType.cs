using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class GenericType : NeslParsingTestEnvironment {

    [Fact]
    public void Generic() {
        CompileSingle(@"
            struct Foo<T> {

                T Method(T t) {
                    return t;
                }

            }
        ");
    }

}