using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing;

public class Using : NeslParsingTestEnvironment {

    [Fact]
    public void One() {
        CompileSingle(@"
            using System;
        ");
    }

    [Fact]
    public void Repeated() {
        CompileSingleThrow(@"
            using System;
            using System;
        ", CompilationErrorType.UsingAlreadyExists);
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrow(@"
            using System325237452342349234234;
        ", CompilationErrorType.UsingNotFound);
    }

}
