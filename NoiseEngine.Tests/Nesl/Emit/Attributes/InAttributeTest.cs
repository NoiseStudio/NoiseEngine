using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class InAttributeTest {

    [Fact]
    public void Create() {
        InAttribute attribute = InAttribute.Create();
        attribute.CheckIsValid();
    }

}
