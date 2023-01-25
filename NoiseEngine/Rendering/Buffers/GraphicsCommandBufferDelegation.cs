using NoiseEngine.Collections;
using NoiseEngine.Common;
using NoiseEngine.Mathematics;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers;

internal abstract class GraphicsCommandBufferDelegation {

    protected readonly SerializationWriter writer;
    protected readonly FastList<object> references;
    protected readonly FastList<IReferenceCoutable> rcReferences;

    protected GraphicsCommandBufferDelegation(
        SerializationWriter writer, FastList<object> references, FastList<IReferenceCoutable> rcReferences
    ) {
        this.writer = writer;
        this.references = references;
        this.rcReferences = rcReferences;
    }

    public abstract void AttachCameraWorker(SimpleCamera camera);

    public abstract void DispatchWorker(ComputeKernel kernel, Vector3<uint> groupCount);

    public abstract void DrawMeshWorker(Mesh mesh, Material material);

}
