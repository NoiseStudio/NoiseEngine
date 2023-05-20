using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void Define() {
        NeslAssembly assembly = CompileSingle(@"
            struct HelloWorld {}
        ");

        Assert.Single(assembly.Types);
        Assert.Equal("HelloWorld", assembly.Types.Single().Name);
    }

    [Fact]
    public void DefineExisting() {
        CompileSingleThrow(@"
            struct Abc {}
            struct Abc {}
        ", CompilationErrorType.TypeAlreadyExists);
    }

}
