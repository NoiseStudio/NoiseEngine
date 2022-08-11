using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.Default;

internal static class Vectors {

    public static NeslType Vector3 { get; }
    public static NeslType Vector4 { get; }

    static Vectors() {
        Vector3 = CreateVector(BuiltInTypes.Float32, 3);
        Vector4 = CreateVector(BuiltInTypes.Float32, 4);
    }

    private static NeslType CreateVector(NeslType type, uint size) {
        NeslTypeBuilder result = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Vector{size}");
        result.AddAttribute(ValueTypeAttribute.Create());
        result.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create(
            null, $"OpTypeVector`{type.FullName}`{size}"));
        result.AddAttribute(SizeAttribute.Create(32 * size));

        return result;
    }

}
