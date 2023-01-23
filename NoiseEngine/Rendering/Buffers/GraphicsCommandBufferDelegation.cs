using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers;

internal abstract class GraphicsCommandBufferDelegation {

    protected readonly SerializationWriter writer;
    protected readonly FastList<object> references;

    protected GraphicsCommandBufferDelegation(SerializationWriter writer, FastList<object> references) {
        this.writer = writer;
        this.references = references;
    }

    public abstract void AttachCameraWorker(SimpleCamera camera);

    public abstract void DispatchWorker(ComputeKernel kernel, Vector3<uint> groupCount);

}
