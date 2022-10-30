using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;

namespace NoiseEngine.Interop.Rendering.Buffers;

internal static partial class GraphicsCommandBufferInterop {

    [InteropImport("rendering_buffers_command_buffer_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsCommandBuffer> handle);

    [InteropImport("rendering_buffers_command_buffer_interop_execute")]
    public static partial InteropResult<InteropHandle<GraphicsFence>> Execute(
        InteropHandle<GraphicsCommandBuffer> handle
    );

}
