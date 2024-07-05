using NoiseEngine.Inputs;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Mathematics;

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

    [InteropImport("rendering_presentation_window_interop_poll_events")]
    public static partial void PollEvents(InteropHandle<Window> handle, InteropHandle<WindowInputRaw> windowInputRaw);

    [InteropImport("rendering_presentation_window_interop_set_position")]
    public static partial InteropResult<None> SetPosition(
        InteropHandle<Window> handle, InteropOption<Vector2<int>> position, InteropOption<Vector2<uint>> size
    );

    [InteropImport("rendering_presentation_window_interop_set_cursor_position")]
    public static partial InteropResult<None> SetCursorPosition(InteropHandle<Window> handle, Vector2<double> position);

    [InteropImport("rendering_presentation_window_interop_set_title")]
    public static partial InteropResult<None> SetTitle(InteropHandle<Window> handle, string title);
    
    [InteropImport("rendering_presentation_window_interop_is_focused")]
    public static partial bool IsFocused(InteropHandle<Window> handle);

}
