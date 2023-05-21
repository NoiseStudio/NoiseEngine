using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void DefineWithType() {
        CompileSingle(@"
            struct Phantom {}

            Phantom Main() {
                return new Phantom();
            }
        ");
    }

    [Fact]
    public void DefineWithNotExistingType() {
        CompileSingleThrow(@"
            adda3457356345 Main() {
                return new adda3457356345();
            }
        ", CompilationErrorType.TypeNotFound, CompilationErrorType.TypeNotFound);
    }

    [Fact]
    public void DefineWithTheSameName() {
        CompileSingleThrow(@"
            void Main() {}
            void Main() {}
        ", CompilationErrorType.MethodAlreadyExists);
    }

    [Fact]
    public void DefineWithTheSameNameButWithDifferentParameters() {
        CompileSingle(@"
            void Main() {}
            void Main(f32 a) {}
        ");
    }

    [Fact]
    public void Intrinsic() {
        NeslMethod method = CompileSingle(@"
            struct Mock {}

            [Intrinsic]
            Mock Main() {
                return new Mock();
            }
        ").Types.SelectMany(x => x.Methods).Single(x => x.Name == "Main");

        Assert.True(method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName));
        Assert.Empty(method.GetIlContainer().GetInstructions());
    }

}
