using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;
using System.Linq;

namespace NoiseEngine.Tests.Nesl;

public class NeslMethodTest {

    [Fact]
    public void MakeGenericFromNotGenericType() {
        const string MethodName = "Method";

        NeslTypeBuilder type = TestEmitHelper.NewType();

        NeslMethodBuilder genericMethod = type.DefineMethod(MethodName);
        NeslGenericTypeParameterBuilder genericTypeParameter = genericMethod.DefineGenericTypeParameter("T");

        genericMethod.SetReturnType(genericTypeParameter);
        genericMethod.SetParameters(genericTypeParameter);

        IlGenerator il = genericMethod.IlGenerator;
        il.Emit(OpCode.Load, 0u, 0u);
        il.Emit(OpCode.Return);

        // Construct final type.
        NeslType genericParameterType = BuiltInTypes.Float32;

        Assert.Throws<ArgumentOutOfRangeException>(() => genericMethod.MakeGeneric());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            genericMethod.MakeGeneric(genericParameterType, genericParameterType));

        NeslMethod method = genericMethod.MakeGeneric(genericParameterType);
        Assert.Equal(method, genericMethod.MakeGeneric(genericParameterType));

        // Check properties.
        CheckMakeGenericProperties(genericMethod, method);
        Assert.Equal(genericParameterType, method.ReturnType);
        Assert.Equal(new NeslType[] { genericParameterType }, method.ParameterTypes);
    }

    [Fact]
    public void MakeGenericFromGenericType() {
        const string MethodName = "Method";

        NeslTypeBuilder genericType = TestEmitHelper.NewType();
        NeslGenericTypeParameterBuilder genericTypeParameter1 = genericType.DefineGenericTypeParameter("T1");

        NeslMethodBuilder genericMethod = genericType.DefineMethod(MethodName);
        NeslGenericTypeParameterBuilder genericTypeParameter2 = genericMethod.DefineGenericTypeParameter("T2");

        genericMethod.SetReturnType(genericTypeParameter2);
        genericMethod.SetParameters(genericTypeParameter2, genericTypeParameter1);

        IlGenerator il = genericMethod.IlGenerator;
        il.Emit(OpCode.Load, 0u, 0u);
        il.Emit(OpCode.Return);

        // Construct final type.
        NeslType genericParameterType1 = BuiltInTypes.Float64;
        NeslType genericParameterType2 = BuiltInTypes.Float32;

        Assert.Throws<InvalidOperationException>(() => genericMethod.MakeGeneric(genericParameterType2));
        NeslType type = genericType.MakeGeneric(genericParameterType1);
        NeslMethod finalGenericMethod = type.GetMethod(MethodName)!;

        Assert.Throws<ArgumentOutOfRangeException>(() => finalGenericMethod.MakeGeneric());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            finalGenericMethod.MakeGeneric(genericParameterType2, genericParameterType2));

        NeslMethod method = finalGenericMethod.MakeGeneric(genericParameterType2);
        Assert.Equal(method, finalGenericMethod.MakeGeneric(genericParameterType2));

        // Check properties.
        CheckMakeGenericProperties(finalGenericMethod, method);
        Assert.Equal(genericParameterType2, method.ReturnType);
        Assert.Equal(new NeslType[] { genericParameterType2, genericParameterType1 }, method.ParameterTypes);
    }

    private void CheckMakeGenericProperties(NeslMethod genericMethod, NeslMethod method) {
        Assert.False(method.IsGeneric);
        Assert.Equal(genericMethod.Name, method.Name);
        Assert.Equal(genericMethod.Type, method.Type);
        Assert.Equal(genericMethod.Attributes.Count(), method.Attributes.Count());
        Assert.Equal(genericMethod.ReturnValueAttributes.Count(), method.ReturnValueAttributes.Count());
        Assert.Equal(genericMethod.ParameterAttributes.Select(x => x.Count()),
            method.ParameterAttributes.Select(x => x.Count()));
    }

}
