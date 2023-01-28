using NoiseEngine.Inputs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Interop.Rendering.Presentation;

internal static partial class WindowInterop {

    [InteropImport("rendering_presentation_window_interop_create")]
    public static partial InteropResult<InteropHandle<Window>> Create(
        ulong id, string title, uint width, uint height, WindowSettingsRaw settings
    );

    [InteropImport("rendering_presentation_window_interop_destroy")]
    public static partial void Destroy(InteropHandle<Window> handle);

    [InteropImport("rendering_presentation_window_interop_dispose")]
    public static partial InteropResult<None> Dispose(InteropHandle<Window> handle);

    [InteropImport("rendering_presentation_window_interop_pool_events")]
    public static partial void PoolEvents(InteropHandle<Window> handle, Span<KeyValue> keyValues);

    [InteropImport("rendering_presentation_window_interop_set_position")]
    public static partial InteropResult<None> SetPosition(
        InteropHandle<Window> handle, InteropOption<Vector2<int>> position, InteropOption<Vector2<uint>> size
    );

}
