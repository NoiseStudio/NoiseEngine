using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl.Emit;

public class NeslMethodBuilderTest {

    [Fact]
    public void AddAttribute() {
        NeslMethodBuilder method = TestEmitHelper.NewMethod();

        Assert.Throws<InvalidOperationException>(() => method.AddAttribute(MockNotUsableAttribute.Create()));
        Assert.Empty(method.Attributes);

        method.AddAttribute(MockUsableAttribute.Create());
        Assert.Single(method.Attributes);
    }

    [Fact]
    public void AddAttributeToReturnValue() {
        NeslMethodBuilder method = TestEmitHelper.NewMethod();

        Assert.Throws<InvalidOperationException>(() =>
            method.AddAttributeToReturnValue(MockNotUsableAttribute.Create()));
        Assert.Empty(method.ReturnValueAttributes);

        method.AddAttributeToReturnValue(MockUsableAttribute.Create());
        Assert.Single(method.ReturnValueAttributes);
    }

    [Theory]
    [InlineData(0)]
    public void AddAttributeToParameter(int parameterIndex) {
        NeslMethodBuilder method = TestEmitHelper.NewMethod();

        Assert.Throws<InvalidOperationException>(() =>
            method.AddAttributeToParameter(parameterIndex, MockNotUsableAttribute.Create()));
        Assert.Empty(method.ParameterAttributes[parameterIndex]);

        method.AddAttributeToParameter(parameterIndex, MockUsableAttribute.Create());
        Assert.Single(method.ParameterAttributes[parameterIndex]);
    }

}
