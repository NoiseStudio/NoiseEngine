namespace NoiseEngine.Interop.Rendering.Presentation;

internal static partial class WindowInterop {

    [InteropImport("rendering_presentation_window_interop_create")]
    public static partial InteropResult<InteropHandle<Window>> Create(string title, uint width, uint height);

    [InteropImport("rendering_presentation_window_interop_destroy")]
    public static partial void Destroy(InteropHandle<Window> handle);

}
