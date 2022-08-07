using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using System.Runtime.InteropServices;

namespace NoiseEngine.Nesl.Default;

internal static class Buffers {

    public static NeslType ReadWriteBuffer { get; }

    static Buffers() {
        ReadWriteBuffer = CreateReadWriteBuffer();
    }

    private static NeslType CreateReadWriteBuffer() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.ReadWriteBuffer");
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create("System.Single[]", null));
        type.AddAttribute(SizeAttribute.Create((uint)Marshal.SizeOf<nuint>() * 8));

        return type;
    }

}
