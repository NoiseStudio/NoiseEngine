using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using System;
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

        // Create generic type definition.
        NeslTypeBuilder genericType = TestEmitHelper.NewType();
        NeslGenericTypeParameterBuilder genericTypeParameter = genericType.DefineGenericTypeParameter("T");

        NeslField genericField = genericType.DefineField(FieldName, genericTypeParameter);

        // Construct final type.
        NeslType genericParameterType = BuiltInTypes.Float32;

        Assert.Throws<ArgumentOutOfRangeException>(() => genericType.MakeGeneric());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            genericType.MakeGeneric(genericParameterType, genericParameterType));

        NeslType type = genericType.MakeGeneric(genericParameterType);

        // Check fields.
        NeslField? field = type.GetField(FieldName);

        Assert.NotNull(field);
        Assert.Equal(genericField.Name, field!.Name);
        Assert.Equal(genericField.Attributes.OrderBy(x => x.FullName), field!.Attributes.OrderBy(x => x.FullName));
        Assert.Equal(type, field.ParentType);
        Assert.Equal(genericParameterType, field.FieldType);
    }

}
