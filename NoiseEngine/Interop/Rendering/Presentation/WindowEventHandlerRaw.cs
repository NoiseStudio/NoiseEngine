using System.Runtime.InteropServices;
using static NoiseEngine.Rendering.Presentation.WindowEventHandler;

namespace NoiseEngine.Interop.Rendering.Presentation;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct WindowEventHandlerRaw(
    UserClosedDelegate UserClosedHandler,
    SizeChangedDelegate SizeChangedHandler
);
