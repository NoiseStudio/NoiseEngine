using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Systems;

namespace NoiseEngine.Rendering;

internal class RenderFrameResources {

    public Camera Camera { get; }
    public MeshRendererResources MeshRendererResources { get; }
    public GraphicsCommandBuffer CommandBuffer { get; }

    public RenderFrameResources(GraphicsDevice device, Camera camera) {
        Camera = camera;
        MeshRendererResources = new MeshRendererResources();
        CommandBuffer = new GraphicsCommandBuffer(device, false);
    }

    public void RecordAndExecute() {
        GraphicsCommandBuffer commandBuffer = CommandBuffer;

        commandBuffer.AttachCameraUnchecked(Camera);
        MeshRendererResources.RecordMeshes(commandBuffer);
        commandBuffer.DetachCameraUnchecked();

        commandBuffer.Execute();
    }

    public void Clear() {
        MeshRendererResources.Clear();
        CommandBuffer.Clear();
    }

}
