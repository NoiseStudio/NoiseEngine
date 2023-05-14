using NoiseEngine.Collections;
using NoiseEngine.Common;
using NoiseEngine.Mathematics;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers;

internal abstract class GraphicsCommandBufferDelegation {

    protected readonly GraphicsCommandBuffer commandBuffer;
    protected readonly SerializationWriter writer;
    protected readonly FastList<object> references;
    protected readonly FastList<IReferenceCoutable> rcReferences;

    protected GraphicsCommandBufferDelegation(
        GraphicsCommandBuffer commandBuffer, SerializationWriter writer, FastList<object> references,
        FastList<IReferenceCoutable> rcReferences
    ) {
        this.commandBuffer = commandBuffer;
        this.writer = writer;
        this.references = references;
        this.rcReferences = rcReferences;
    }

    public abstract void Clear();

    public abstract void AttachCameraWorker(SimpleCamera camera);

    public abstract void DispatchWorker(ComputeKernel kernel, ComputeMaterial material, Vector3<uint> groupCount);

    public abstract void DrawMeshWorker(Mesh mesh, Material material, Matrix4x4<float> transform);

}
