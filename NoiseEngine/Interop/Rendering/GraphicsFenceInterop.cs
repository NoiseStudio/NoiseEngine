﻿using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Interop.Rendering;

internal static partial class GraphicsFenceInterop {

    [InteropImport("rendering_fence_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsFence> handle);

    [InteropImport("rendering_fence_interop_wait")]
    public static partial InteropResult<InteropBool> Wait(InteropHandle<GraphicsFence> fence, ulong nanosecondsTimeout);

    [InteropImport("rendering_fence_interop_is_signaled")]
    public static partial InteropResult<InteropBool> IsSignaled(InteropHandle<GraphicsFence> fence);

    [InteropImport("rendering_fence_interop_wait_multiple")]
    public static partial InteropResult<InteropBool> WaitMultiple(
        ReadOnlySpan<InteropHandle<GraphicsFence>> fences, bool waitAll, ulong nanosecondsTimeout
    );

}
