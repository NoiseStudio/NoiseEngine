using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Threading;
using System;

namespace NoiseEngine;

public class Window : IDisposable {

    private AtomicBool isDisposed;

    public bool IsDisposed => isDisposed;

    private InteropHandle<Window> Handle { get; }

    public Window(string? title = null, uint width = 1280, uint height = 720) {
        title ??= Application.Name;

        if (!WindowInterop.Create(title, width, height).TryGetValue(
            out InteropHandle<Window> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = handle;
    }

    ~Window() {
        WindowInterop.Destroy(Handle);
    }

    /// <summary>
    /// Disposes this <see cref="Window"/>.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        WindowInterop.Destroy(Handle);
        GC.SuppressFinalize(this);
    }

}
