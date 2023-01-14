using NoiseEngine.Interop.InteropMarshalling;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Presentation;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct WindowSettingsRaw(
    WindowMode Mode,
    WindowControls Controls,
    WindowPosition Position,
    InteropBool Resizable
) {

    public WindowSettingsRaw(WindowSettings s) : this(s.Mode, s.Controls, s.Position, s.Resizable) {
    }

}
