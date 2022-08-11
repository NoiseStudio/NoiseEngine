using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.Default;

internal static class Manager {

    public static NeslAssemblyBuilder AssemblyBuilder { get; }

    static Manager() {
        AssemblyBuilder = NeslAssemblyBuilder.DefineAssembly("System");
    }

}
