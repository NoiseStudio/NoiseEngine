using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl;

public class NeslTypeTest {

    [Fact]
    public void Properties() {
        const string Namespace = "Quick.Brown";
        const string TypeName = "Fox";
        const string FullName = $"{Namespace}.{TypeName}";

        NeslTypeBuilder type = TestEmitHelper.NewAssembly().DefineType(FullName);
        type.AddAttribute(ValueTypeAttribute.Create());

        Assert.False(type.IsClass);
        Assert.True(type.IsValueType);

        Assert.Equal(FullName, type.FullName);
        Assert.Equal(Namespace, type.Namespace);
        Assert.Equal(TypeName, type.Name);
    }

}
