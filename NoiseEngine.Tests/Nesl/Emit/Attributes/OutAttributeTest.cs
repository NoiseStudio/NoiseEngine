using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class OutAttributeTest {

    [Fact]
    public void Create() {
        OutAttribute attribute = OutAttribute.Create();
        attribute.CheckIsValid();
    }

}
