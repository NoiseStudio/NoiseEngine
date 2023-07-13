using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class DefineVariable : NeslParsingTestEnvironment {

    [Fact]
    public void DefineWithoutAssignment() {
        CompileSingle(@"
            f32 Main(f32 a) {
                f32 temp;
                temp = a;
                return temp;
            }
        ");
    }

    [Fact]
    public void DefineWithAssignment() {
        CompileSingle(@"
            f32v3 Main(f32v3 a) {
                f32v3 temp = a;
                return temp;
            }
        ");
    }

    [Fact]
    public void DefineWithoutAssignmentMultipleWithSameName() {
        CompileSingleThrow(@"
            f32v3 Main(f32v3 a) {
                f32v3 temp = a;
                f32v3 temp = a;
                return temp;
            }
        ", CompilationErrorType.VariableAlreadyExists);
    }

    [Fact]
    public void DefineWithAssignmentMultipleWithSameName() {
        CompileSingleThrow(@"
            f32v3 Main(f32v3 a) {
                f32v3 temp = a;
                f32v3 temp = a;
                return temp;
            }
        ", CompilationErrorType.VariableAlreadyExists);
    }

}
