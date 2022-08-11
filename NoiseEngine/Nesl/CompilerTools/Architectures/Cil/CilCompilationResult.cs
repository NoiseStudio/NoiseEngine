using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.Cil;

internal class CilCompilationResult {

    public Assembly Assembly { get; }

    internal CilCompilationResult(Assembly assembly) {
        Assembly = assembly;
    }

}
