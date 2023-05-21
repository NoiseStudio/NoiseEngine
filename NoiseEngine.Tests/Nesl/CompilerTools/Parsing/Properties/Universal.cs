using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Properties;

public class Universal : NeslParsingTestEnvironment {

    [Fact]
    public void Intrinsic() {
        NeslType type = CompileSingle(@"
            local static class Box {
                [Intrinsic]
                public static f32 TestProperty { get; }
            }
        ").Types.Single();

        string methodName = NeslOperators.PropertyGet + "TestProperty";
        NeslMethod method = type.Methods.Single(x => x.Name == methodName);

        Assert.True(method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName));
        Assert.Empty(method.GetIlContainer().GetInstructions());

        Assert.Empty(type.Fields);
    }

}
