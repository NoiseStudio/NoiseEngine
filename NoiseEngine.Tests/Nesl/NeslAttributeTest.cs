using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl;

public class NeslAttributeTest {

    [Fact]
    public void Cast() {
        SizeAttribute attribute = SizeAttribute.Create(32);
        NeslAttribute serializedAttribute =
            NeslAttribute.Create(attribute.FullName, attribute.Targets, attribute.Bytes.AsSpan().ToArray());

        Assert.IsType<SizeAttribute>(attribute.Cast<SizeAttribute>());
        Assert.IsType<SizeAttribute>(serializedAttribute.Cast<SizeAttribute>());
    }

    [Fact]
    public void TryCast() {
        SizeAttribute attribute = SizeAttribute.Create(32);
        NeslAttribute serializedAttribute =
            NeslAttribute.Create(attribute.FullName, attribute.Targets, attribute.Bytes.AsSpan().ToArray());
        NeslAttribute serializedAttribute2 =
            NeslAttribute.Create("Hello!", attribute.Targets, attribute.Bytes.AsSpan().ToArray());

        Assert.True(attribute.TryCast<SizeAttribute>(out _));
        Assert.True(serializedAttribute.TryCast<SizeAttribute>(out _));
        Assert.False(serializedAttribute2.TryCast<SizeAttribute>(out _));
    }

}
