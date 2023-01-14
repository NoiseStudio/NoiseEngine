using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Threading;
using System;

namespace NoiseEngine;

public class Window : IDisposable, ICameraRenderTarget {

    private AtomicBool isDisposed;

    public bool IsDisposed => isDisposed;
    public uint Width { get; }
    public uint Height { get; }

    internal InteropHandle<Window> Handle { get; }

    TextureUsage ICameraRenderTarget.Usage => TextureUsage.ColorAttachment;
    Vector3<uint> ICameraRenderTarget.Extent => new Vector3<uint>(Width, Height, 0);
    uint ICameraRenderTarget.SampleCount => 1;
    TextureFormat ICameraRenderTarget.Format => throw new NotImplementedException();

    public Window(string? title, uint width, uint height, WindowSettings settings) {
        title ??= Application.Name;

        Width = width;
        Height = height;

        if (!WindowInterop.Create(title, width, height, new WindowSettingsRaw(settings)).TryGetValue(
            out InteropHandle<Window> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = handle;
    }

    public Window(string? title = null, uint width = 1280, uint height = 720) : this(
        title, width, height, new WindowSettings()
    ) {
    }

    ~Window() {
        WindowInterop.Destroy(Handle);
    }

    internal static WindowApi GetWindowApi() {
        if (OperatingSystem.IsWindows())
            return WindowApi.WindowsApi;
        return WindowApi.None;
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
