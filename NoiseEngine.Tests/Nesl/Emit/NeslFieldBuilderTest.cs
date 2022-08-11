using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl.Emit;

public class NeslFieldBuilderTest {

    [Fact]
    public void AddAttribute() {
        NeslFieldBuilder field = TestEmitHelper.NewField();

        Assert.Throws<InvalidOperationException>(() => field.AddAttribute(MockNotUsableAttribute.Create()));
        Assert.Empty(field.Attributes);

        field.AddAttribute(MockUsableAttribute.Create());
        Assert.Single(field.Attributes);
    }

}
