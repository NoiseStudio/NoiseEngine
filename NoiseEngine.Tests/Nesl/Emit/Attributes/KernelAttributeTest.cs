using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class KernelAttributeTest {

    [Theory]
    [InlineData(68, 18, 64)]
    [InlineData(uint.MaxValue, uint.MaxValue / 2, 0)]
    public void Create(uint x, uint y, uint z) {
        Vector3<uint> localSize = new Vector3<uint>(x, y, z);

        KernelAttribute attribute = KernelAttribute.Create(localSize);
        attribute.AssertValid();
        Assert.Equal(localSize, attribute.LocalSize);
    }

}
