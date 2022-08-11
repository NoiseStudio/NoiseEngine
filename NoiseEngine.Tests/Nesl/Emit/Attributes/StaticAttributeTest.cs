using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class StaticAttributeTest {

    [Fact]
    public void Create() {
        StaticAttribute attribute = StaticAttribute.Create();
        attribute.CheckIsValid();
    }

}
