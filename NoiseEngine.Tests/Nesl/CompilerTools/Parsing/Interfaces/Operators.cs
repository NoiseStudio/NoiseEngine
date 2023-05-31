using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Operators : NeslParsingTestEnvironment {

    [Fact]
    public void Addition() {
        CompileSingle(@"
            interface IFoo<T> {
                static T operator +(T a, T b);
            }
        ");
    }

    [Fact]
    public void Assignment() {
        CompileSingleThrow(@"
            interface IFoo<T> {
                static T operator *=(T a, T b);
            }
        ", CompilationErrorType.AssignmentOperatorDeclarationNotAllowed);
    }

}
