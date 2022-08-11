using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class SizeAttributeTest {

    [Theory]
    [InlineData(68)]
    public void Create(ulong size) {
        SizeAttribute attribute = SizeAttribute.Create(size);
        attribute.AssertValid();
        Assert.Equal(size, attribute.Size);
    }

}
