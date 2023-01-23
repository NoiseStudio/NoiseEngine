using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Rendering.Presentation.Events;
using NoiseEngine.Threading;
using System;
using System.Threading;

namespace NoiseEngine;

public class Window : IDisposable, ICameraRenderTarget {

    private static ulong nextId;

    private readonly object assignedCameraLocker = new object();

    private AtomicBool isDisposed;
    private SimpleCamera? assignedCamera;

    public bool IsDisposed => isDisposed;
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    internal ulong Id { get; }
    internal InteropHandle<Window> Handle { get; }
    internal object PoolEventsLocker { get; } = new object();

    TextureUsage ICameraRenderTarget.Usage => TextureUsage.ColorAttachment;
    Vector3<uint> ICameraRenderTarget.Extent => new Vector3<uint>(Width, Height, 0);
    uint ICameraRenderTarget.SampleCount => 1;
    TextureFormat ICameraRenderTarget.Format => throw new NotImplementedException();

    public event EventHandler<SizeChangedEventArgs>? SizeChanged;

    public Window(string? title, uint width, uint height, WindowSettings settings) {
        title ??= Application.Name;

        Width = width;
        Height = height;

        Id = Interlocked.Increment(ref nextId);
        WindowEventHandler.InitializeStatic();

        if (!WindowInterop.Create(Id, title, width, height, new WindowSettingsRaw(settings)).TryGetValue(
            out InteropHandle<Window> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = handle;
        WindowEventHandler.RegisterWindow(this);
    }

    public Window(string? title = null, uint width = 1280, uint height = 720) : this(
        title, width, height, new WindowSettings()
    ) {
    }

    ~Window() {
        if (Handle == InteropHandle<Window>.Zero)
            return;

        WindowEventHandler.UnregisterWindow(Id);
        if (!WindowInterop.Destroy(Handle).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
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

        WindowEventHandler.UnregisterWindow(Id);

        GC.SuppressFinalize(this);
        if (!WindowInterop.Destroy(Handle).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

    /// <summary>
    /// Resizes this <see cref="Window"/>.
    /// </summary>
    /// <param name="width">New width.</param>
    /// <param name="height">New height.</param>
    public void Resize(uint width, uint height) {
        WindowInterop.SetPosition(Handle, null, new Vector2<uint>(width, height));
    }

    internal void ChangeAssignedCamera(SimpleCamera? camera) {
        lock (assignedCameraLocker) {
            if (assignedCamera is not null)
                assignedCamera.RenderTarget = null;
            assignedCamera = camera;
        }
    }

    internal void RaiseUserClosed() {
        Dispose();
    }

    internal void RaiseSizeChanged(uint newWidth, uint newHeight) {
        SizeChangedEventArgs args = new SizeChangedEventArgs {
            OldWidth = Width,
            OldHeight = Height,
            NewWidth = newWidth,
            NewHeight = newHeight
        };

        SizeChanged?.Invoke(this, args);
        if (args.Cancel)
            return;

        Width = newWidth;
        Height = newHeight;
    }

}
