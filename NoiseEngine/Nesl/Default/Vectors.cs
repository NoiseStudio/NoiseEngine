using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class Vectors {

    public const string Vector3Name = "System::System.Vector3`1";
    public const string Vector4Name = "System::System.Vector4`1";

    private static readonly string[] units = new string[] { "X", "Y", "Z", "W", "V" };

    private static readonly NeslType vector3;
    private static readonly NeslType vector4;

    static Vectors() {
        vector3 = CreateVector(3);
        vector4 = CreateVector(4);
    }

    public static NeslType GetVector3(NeslType type) {
        return vector3.MakeGeneric(type);
    }

    public static NeslType GetVector4(NeslType type) {
        return vector4.MakeGeneric(type);
    }

    private static NeslType CreateVector(uint size) {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Vector{size}`1");
        NeslGenericTypeParameterBuilder genericTypeParameter = type.DefineGenericTypeParameter("T");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create(
            $"OpTypeVector`{{&{genericTypeParameter.Name}}}`{size}"
        ));

        for (int i = 0; i < size; i++)
            type.DefineField(units[i], genericTypeParameter);

        if (size == 4) {
            NeslMethodBuilder method = type.DefineMethod(
                NeslOperators.Constructor, type, GetVector3(genericTypeParameter), genericTypeParameter
            );
            IlGenerator il = method.IlGenerator;
            il.Emit(OpCode.DefVariable, type);
            il.Emit(OpCode.SetField, 6u, 3u, 5u);
            il.Emit(OpCode.LoadField, 5u, 4u, 0u);
            il.Emit(OpCode.SetField, 6u, 0u, 5u);
            il.Emit(OpCode.LoadField, 5u, 4u, 1u);
            il.Emit(OpCode.SetField, 6u, 1u, 5u);
            il.Emit(OpCode.LoadField, 5u, 4u, 2u);
            il.Emit(OpCode.SetField, 6u, 2u, 5u);
            il.Emit(OpCode.ReturnValue, 6u);
        }

        return type;
    }

}
