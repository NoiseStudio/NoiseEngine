using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class BuiltInTypes {

    public static NeslType Float32 { get; }

    static BuiltInTypes() {
        Float32 = CreateFloat32();
    }

    private static NeslType CreateFloat32() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Float32");
        type.AddAttribute(ValueTypeAttribute.Create());
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("System.Single", null));
        type.AddAttribute(SizeAttribute.Create(32));

        return type;
    }

}
