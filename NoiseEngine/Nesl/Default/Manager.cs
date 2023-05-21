using NoiseEngine.Nesl.Emit;
using System;
using System.IO;

namespace NoiseEngine.Nesl.Default;

internal static class Manager {

    private static readonly object locker = new object();
    private static WeakReference<NeslAssembly>? assembly;

    public static NeslAssemblyBuilder AssemblyBuilder { get; }

    public static NeslAssembly Assembly {
        get {
            WeakReference<NeslAssembly>? weak = Manager.assembly;
            if (weak is not null && weak.TryGetTarget(out NeslAssembly? assembly))
                return assembly;

            lock (locker) {
                weak = Manager.assembly;
                if (weak is not null && weak.TryGetTarget(out assembly))
                    return assembly;

                assembly = NeslAssembly.LoadWithoutDefault(File.ReadAllBytes("Resources/Shaders/System.nesil"));
                Manager.assembly = new WeakReference<NeslAssembly>(assembly);
                return assembly;
            }
        }
    }

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
