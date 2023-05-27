using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Indexers;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void IntrinsicOnlyGetter() {
        NeslType type = CompileSingle(@"
            [Intrinsic]
            f32 this[u32 i] { get; }
        ").Types.Single();

        string methodName = NeslOperators.IndexerGet;
        NeslMethod method = type.Methods.Single(x => x.Name == methodName);

        Assert.True(method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName));
        Assert.Empty(method.GetIlContainer().GetInstructions());

        Assert.Empty(type.Fields);
    }

    [Fact]
    public void IntrinsicGetterAndSetter() {
        NeslType type = CompileSingle(@"
            [Intrinsic]
            f32 this[u32 i] { get; set; }
        ").Types.Single();

        // Getter.
        string methodName = NeslOperators.IndexerGet;
        NeslMethod method = type.Methods.Single(x => x.Name == methodName);

        Assert.True(method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName));
        Assert.Empty(method.GetIlContainer().GetInstructions());

        // Setter.
        methodName = NeslOperators.IndexerSet;
        method = type.Methods.Single(x => x.Name == methodName);

        Assert.True(method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName));
        Assert.Empty(method.GetIlContainer().GetInstructions());

        Assert.Empty(type.Fields);
    }

    [Fact]
    public void IntrinsicGetterAndInitializer() {
        CompileSingleThrow(@"
            [Intrinsic]
            f32 this[u32 i] { get; init; }
        ", CompilationErrorType.InitializerForIndexerNotAllowed);
    }

}
