using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods.New;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void ExistingConstructor() {
        CompileSingle(@"
            struct Phantom {}

            Phantom Main() {
                return new Phantom();
            }
        ");
    }

    [Fact]
    public void NotExistingConstructor() {
        CompileSingleThrow(@"
            struct Phantom {}

            Phantom Main() {
                return new Phantom(new Phantom());
            }
        ", CompilationErrorType.ConstructorNotFound);
    }

}
