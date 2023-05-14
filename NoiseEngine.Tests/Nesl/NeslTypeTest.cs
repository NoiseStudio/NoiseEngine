using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Nesl;

public class NeslTypeTest {

    [Fact]
    public void Properties() {
        const string Namespace = "Quick.Brown";
        const string TypeName = "Fox";
        const string FullName = $"{Namespace}.{TypeName}";

        NeslTypeBuilder type = TestEmitHelper.NewAssembly().DefineType(FullName);
        type.AddAttribute(ValueTypeAttribute.Create());

        Assert.False(type.IsClass);
        Assert.True(type.IsValueType);

        Assert.Equal(FullName, type.FullName);
        Assert.Equal(Namespace, type.Namespace);
        Assert.Equal(TypeName, type.Name);
    }

    [Fact]
    public void MakeGeneric() {
        const string FieldName = "Field";
        const string MethodName = "Method";

        // Create generic type definition.
        NeslTypeBuilder genericType = TestEmitHelper.NewType();
        NeslGenericTypeParameterBuilder genericTypeParameter = genericType.DefineGenericTypeParameter("T");

        NeslField genericField = genericType.DefineField(FieldName, genericTypeParameter);

        NeslMethodBuilder genericMethod =
            genericType.DefineMethod(MethodName, genericType, Buffers.GetReadWriteBuffer(genericTypeParameter));
        IlGenerator il = genericMethod.IlGenerator;

        il.Emit(OpCode.Load, 0u, 0u);
        il.Emit(OpCode.DefVariable, genericType);
        il.Emit(OpCode.ReturnValue, 1u);

        // Construct final type.
        NeslType genericParameterType = BuiltInTypes.Float32;

        Assert.Throws<ArgumentOutOfRangeException>(() => genericType.MakeGeneric());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            genericType.MakeGeneric(genericParameterType, genericParameterType));

        NeslType type = genericType.MakeGeneric(genericParameterType);
        Assert.Equal(type, genericType.MakeGeneric(genericParameterType));

        // Check properties.
        Assert.False(type.IsGeneric);
        Assert.Equal(genericType.Name, type.Name);

        // Check fields.
        NeslField? field = type.GetField(FieldName);

        Assert.NotNull(field);
        Assert.Equal(genericField.Name, field!.Name);
        Assert.Equal(genericField.Attributes.Count(), field.Attributes.Count());
        Assert.Equal(type, field.ParentType);
        Assert.Equal(genericParameterType, field.FieldType);

        // Check methods.
        NeslMethod? method = type.GetMethod(MethodName);

        Assert.NotNull(method);
        Assert.Equal(genericMethod.Name, method!.Name);
        Assert.Equal(type, method.ReturnType);
        Assert.Equal(genericMethod.Attributes.Count(), method.Attributes.Count());
        Assert.Equal(genericMethod.ReturnValueAttributes.Count(), method.ReturnValueAttributes.Count());
        Assert.Equal(genericMethod.ParameterAttributes.Select(x => x.Count()),
            method.ParameterAttributes.Select(x => x.Count()));

        // Check method body.
        Assert.Equal(
            genericMethod.GetIlContainer().GetInstructions().Count(),
            method.GetIlContainer().GetInstructions().Count()
        );

        IEnumerator<Instruction> instructions = method.GetIlContainer().GetInstructions().GetEnumerator();
        Assert.True(instructions.MoveNext());
        Assert.Equal(OpCode.Load, instructions.Current.OpCode);
        Assert.Equal(0u, instructions.Current.ReadUInt32());
        Assert.Equal(0u, instructions.Current.ReadUInt32());

        Assert.True(instructions.MoveNext());
        Assert.Equal(OpCode.DefVariable, instructions.Current.OpCode);
        Assert.Equal(type.Assembly.GetLocalTypeId(type), instructions.Current.ReadUInt64());

        Assert.True(instructions.MoveNext());
        Assert.Equal(OpCode.ReturnValue, instructions.Current.OpCode);
        Assert.Equal(1u, instructions.Current.ReadUInt32());
    }

}
