using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl.Emit;

public class NeslTypeBuilderTest {

    [Fact]
    public void DefineField() {
        const string FieldNameA = "Example";
        const string FieldNameB = "Plane";

        NeslTypeBuilder type = TestEmitHelper.NewType();

        type.DefineField(FieldNameA, BuiltInTypes.Float32);
        Assert.Throws<ArgumentException>(() => type.DefineField(FieldNameA, BuiltInTypes.Float32));
        Assert.Throws<ArgumentException>(() => type.DefineField(FieldNameA, BuiltInTypes.Float64));

        type.DefineField(FieldNameB, BuiltInTypes.Float32);
    }

    [Fact]
    public void DefineMethod() {
        const string MethodNameA = "Example";
        const string MethodNameB = "Plane";

        NeslTypeBuilder type = TestEmitHelper.NewType();

        type.DefineMethod(MethodNameA);
        Assert.Throws<ArgumentException>(() => type.DefineMethod(MethodNameA));
        Assert.Throws<ArgumentException>(() => type.DefineMethod(MethodNameA, BuiltInTypes.Float32));

        type.DefineMethod(MethodNameA, null, BuiltInTypes.Float32);
        Assert.Throws<ArgumentException>(() => type.DefineMethod(MethodNameA, null, BuiltInTypes.Float32));

        type.DefineMethod(MethodNameA, null, BuiltInTypes.Float64);
        Assert.Throws<ArgumentException>(() => type.DefineMethod(MethodNameA, null, BuiltInTypes.Float64));

        type.DefineMethod(MethodNameB);
    }

    [Fact]
    public void AddAttribute() {
        NeslTypeBuilder type = TestEmitHelper.NewType();

        Assert.Throws<InvalidOperationException>(() => type.AddAttribute(MockNotUsableAttribute.Create()));
        Assert.Empty(type.Attributes);

        type.AddAttribute(MockUsableAttribute.Create());
        Assert.Single(type.Attributes);
    }

}
