using NoiseEngine.Interop.Rendering.Presentation;

namespace NoiseEngine;

public class Window {

    public Window(string? title = null, uint width = 960, uint height = 540) {
        title ??= Application.Name;

        WindowInterop.Create(title, width, height);
    }

}
