using NoiseEngine.Mathematics;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers;

internal abstract class GraphicsCommandBufferDelegation {

    protected readonly SerializationWriter writer;

    protected GraphicsCommandBufferDelegation(SerializationWriter writer) {
        this.writer = writer;
    }

    public abstract void DispatchWorker(ComputeKernel kernel, Vector3<uint> groupCount);

}
