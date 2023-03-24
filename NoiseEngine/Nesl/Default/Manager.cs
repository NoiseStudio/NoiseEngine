using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.Default;

internal static class Manager {

    public static NeslAssemblyBuilder AssemblyBuilder { get; }

    static Manager() {
        AssemblyBuilder = NeslAssemblyBuilder.DefineAssemblyWithoutDefault("System");

        _ = Buffers.GetReadWriteBuffer(BuiltInTypes.Float32);
        _ = BuiltInTypes.Float32;
        _ = Compute.GlobalInvocation3;
        _ = Matrices.GetMatrix4x4(BuiltInTypes.Float32);
        _ = Vectors.GetVector4(BuiltInTypes.Float32);
        _ = Vertex.ObjectToClipPos;
    }

}
