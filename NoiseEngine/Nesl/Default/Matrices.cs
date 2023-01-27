using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class Matrices {

    public const string Matrix4x4Name = "System::System.Matrix4x4`1";

    private static readonly NeslType matrix4x4;

    static Matrices() {
        matrix4x4 = CreateMatrix(4);
    }

    public static NeslType GetMatrix4x4(NeslType type) {
        return matrix4x4.MakeGeneric(type);
    }

    private static NeslType CreateMatrix(uint size) {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Matrix4x{size}`1");
        NeslGenericTypeParameterBuilder genericTypeParameter = type.DefineGenericTypeParameter("T");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create(
            $"OpTypeMatrix4`{{&{genericTypeParameter.Name}}}`{size}"
        ));

        for (int i = 0; i < size; i++)
            type.DefineField($"C{i}", genericTypeParameter);

        return type;
    }

}
