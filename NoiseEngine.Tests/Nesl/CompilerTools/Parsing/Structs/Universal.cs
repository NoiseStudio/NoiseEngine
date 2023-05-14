using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void DefineExisting() {
        CompileSingleThrow(@"
            struct Abc {}
            struct Abc {}
        ", CompilationErrorType.TypeAlreadyExists);
    }

}
