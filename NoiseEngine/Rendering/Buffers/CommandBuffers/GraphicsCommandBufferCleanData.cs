using NoiseEngine.Collections;
using NoiseEngine.Common;
using NoiseEngine.Interop;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal readonly record struct GraphicsCommandBufferCleanData(
    GraphicsDevice Device, InteropHandle<GraphicsCommandBuffer> Handle, GCHandle GcHandle, FastList<object> References,
    FastList<IReferenceCoutable> RcReferences, FastList<GraphicsFence> Fences, uint ObtainedTimeouts = 0
);
