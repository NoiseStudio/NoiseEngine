namespace NoiseEngine.Interop.Rendering.Presentation;

internal static partial class WindowInterop {

    [InteropImport("rendering_presentation_window_interop_create")]
    public static partial void Create(string title, uint width, uint height);

}
