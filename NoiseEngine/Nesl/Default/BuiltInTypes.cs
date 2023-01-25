using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class BuiltInTypes {

    public const string UInt32Name = "System::System.UInt32";
    public const string Int32Name = "System::System.Int32";
    public const string Float32Name = "System::System.Float32";
    public const string Float64Name = "System::System.Float64";

    public static NeslType UInt32 { get; }
    public static NeslType Int32 { get; }
    public static NeslType Float32 { get; }
    public static NeslType Float64 { get; }

    static BuiltInTypes() {
        UInt32 = CreateUInt32();
        Int32 = CreateInt32();
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

    private static NeslType CreateInt32() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Int32");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("OpTypeInt`32`1"));
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
