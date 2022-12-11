using System.Threading;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVDescriptorSet {

    private uint binding;

    public uint NextBinding() {
        return Interlocked.Increment(ref binding) - 1;
    }

}
