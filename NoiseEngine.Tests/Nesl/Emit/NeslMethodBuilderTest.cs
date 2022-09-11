using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl.Emit;

public class NeslMethodBuilderTest {

    [Fact]
    public void SetParameters() {
        NeslMethodBuilder method = TestEmitHelper.NewType()
            .DefineMethod(nameof(SetParameters), null, BuiltInTypes.Float32);
        method.AddAttributeToParameter(0, MockUsableAttribute.Create());

        Assert.Single(method.ParameterTypes);
        Assert.Equal(BuiltInTypes.Float32, method.ParameterTypes[0]);
        Assert.Single(method.ParameterAttributes[0]);

        method.SetParameters(BuiltInTypes.Float64);
        Assert.Single(method.ParameterTypes);
        Assert.Equal(BuiltInTypes.Float64, method.ParameterTypes[0]);
        Assert.Empty(method.ParameterAttributes[0]);

        method.AddAttributeToParameter(0, MockUsableAttribute.Create());

        method.SetParameters(BuiltInTypes.Float32, BuiltInTypes.Float64);
        Assert.Equal(new NeslType[] { BuiltInTypes.Float32, BuiltInTypes.Float64 }, method.ParameterTypes);
        Assert.Empty(method.ParameterAttributes[0]);
    }

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
