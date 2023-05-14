using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Fields : NeslParsingTestEnvironment {

    [Fact]
    public void DefineEmpty() {
        CompileSingle(@"
            struct Abc {}
        ");
    }

    [Fact]
    public void DefineWithOneField() {
        CompileSingle(@"
            struct Abc {
                f32 Value;
            }
        ");
    }

    [Fact]
    public void DefineWithSeveralFields() {
        CompileSingle(@"
            struct Abc {
                f32 Value;
                f32v3 Vector;
            }
        ");
    }

    [Fact]
    public void DefineWithOneNotFoundTypeField() {
        CompileSingleThrow(@"
            struct Abc {
                Hs664s Value;
            }
        ", CompilationErrorType.TypeNotFound);
    }

    [Fact]
    public void DefineWithOneFieldWithoutSemicolon() {
        CompileSingleThrowAny(@"
            struct Abc {
                f32 Value
            }
        ");
    }

}
