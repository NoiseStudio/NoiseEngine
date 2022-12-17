using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class BuiltInTypes {

    public static NeslType UInt32 { get; }
    public static NeslType Float32 { get; }
    public static NeslType Float64 { get; }

    static BuiltInTypes() {
        UInt32 = CreateUInt32();
        Float32 = CreateFloat32();
        Float64 = CreateFloat64();
    }

    private static NeslType CreateUInt32() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.UInt32");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("OpTypeInt`32`0"));
        type.AddAttribute(SizeAttribute.Create(32));

        return type;
    }

    private static NeslType CreateFloat32() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Float32");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("OpTypeFloat`32"));
        type.AddAttribute(SizeAttribute.Create(32));

        return type;
    }

    private static NeslType CreateFloat64() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Float64");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("OpTypeFloat`64"));
        type.AddAttribute(SizeAttribute.Create(64));

        return type;
    }

}
