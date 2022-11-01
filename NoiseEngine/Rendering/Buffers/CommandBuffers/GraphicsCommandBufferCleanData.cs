using NoiseEngine.Collections;
using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal readonly record struct GraphicsCommandBufferCleanData(
    GraphicsDevice Device, InteropHandle<GraphicsCommandBuffer> Handle, FastList<object> References,
    FastList<GraphicsFence> Fences, uint ObtainedTimeouts = 0
);
