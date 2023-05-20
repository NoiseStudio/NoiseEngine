using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing;

public class Attribute : NeslParsingTestEnvironment {

    [Fact]
    public void Empty() {
        CompileSingleThrowAny(@"
            []
            struct Abc {}
        ");
    }

    [Fact]
    public void WithoutParameters() {
        CompileSingleThrow(@"
            [AttributeNameABDGG]
            struct Abc {}
        ", CompilationErrorType.AttributeNotFound);
    }

    [Fact]
    public void WithParameters() {
        NeslAssembly assembly = CompileSingle(@"
            [Size(2137)]
            struct Jagod {}
        ");

        NeslType type = assembly.Types.Single(x => x.Name == "Jagod");
        SizeAttribute mock = SizeAttribute.Create(1);
        NeslAttribute attribute = type.Attributes.Single(x => x.FullName == mock.FullName);
        Assert.True(attribute.TryCast(out SizeAttribute? finalAttribute));
        Assert.NotNull(finalAttribute);
        Assert.Equal(2137u, finalAttribute!.Size);
    }

    [Fact]
    public void WithInvalidParameters() {
        CompileSingleThrowAny(@"
            [Size(2137, -6)]
            struct Jagod {}
        ");
    }

    [Fact]
    public void TokensAfterParamters() {
        CompileSingleThrowAny(@"
            [Size(2137);]
            struct Jagod {}
        ");
    }

}
