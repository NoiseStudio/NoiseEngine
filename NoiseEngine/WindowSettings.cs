namespace NoiseEngine;

public readonly record struct WindowSettings(
    WindowMode Mode = WindowMode.Windowed,
    WindowControls Controls = WindowControls.All,
    WindowPosition Position = default,
    bool Resizable = true
) {

    public WindowSettings() : this(Mode: WindowMode.Windowed) {
    }

}
