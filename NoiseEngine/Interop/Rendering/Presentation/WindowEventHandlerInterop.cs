namespace NoiseEngine.Interop.Rendering.Presentation;

internal static partial class WindowEventHandlerInterop {

    [InteropImport("rendering_presentation_window_event_handler_interop_initialize")]
    public static partial InteropResult<None> Initialize(WindowEventHandlerRaw handler);

}