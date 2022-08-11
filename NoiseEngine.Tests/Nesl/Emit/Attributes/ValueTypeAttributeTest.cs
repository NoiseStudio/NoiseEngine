using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class ValueTypeAttributeTest {

    [Fact]
    public void Create() {
        ValueTypeAttribute attribute = ValueTypeAttribute.Create();
        attribute.CheckIsValid();
    }

}
