using NoiseEngine.Collections;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class AttributeHelperTest {

    [Theory]
    [InlineData("Awesome")]
    [InlineData(null)]
    public void ReadString(string? value) {
        FastList<byte> buffer = new FastList<byte>();
        AttributeHelper.WriteString(buffer, value);

        Assert.Equal(value, AttributeHelper.ReadString(buffer.AsSpan()));;
    }

}
