using NoiseEngine.Common;
using NoiseEngine.Inputs;
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

public class Window : IDisposable, ICameraRenderTarget, IReferenceCoutable {

    private static ulong nextId;

    private readonly object assignedCameraLocker = new object();

    private AtomicBool isDisposed;
    private long referenceCount = 1;
    private AtomicBool isReleased;
    private SimpleCamera? assignedCamera;
    private string title;

    public WindowInput Input { get; }
    public bool IsDisposed => isDisposed;
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public string Title {
        get => title;
        set {
            if (!ReferenceCoutable.TryRcRetain())
                return;
            
            InteropResult<None> result = WindowInterop.SetTitle(Handle, value);
            ReferenceCoutable.RcRelease();

            if (result.TryGetValue(out _, out ResultError error))
                title = value;
            else
                error.ThrowAndDispose();
        }
    }

    public bool IsFocused { get; private set; }

    internal ulong Id { get; }
    internal InteropHandle<Window> Handle { get; private set; }
    internal object PoolEventsLocker { get; } = new object();

    private IReferenceCoutable ReferenceCoutable => this;

    Vector3<uint> ICameraRenderTarget.Extent => new Vector3<uint>(Width, Height, 1);

    public event EventHandler<WindowDisposedEventArgs>? Disposed;
    public event EventHandler<FocusedEventArgs>? Focused;
    public event EventHandler<UnfocusedEventArgs>? Unfocused;
    public event EventHandler<SizeChangedEventArgs>? SizeChanged;

    public Window(string? title, uint width, uint height, WindowSettings settings) {
        title ??= Application.Name;

        Width = width;
        Height = height;
        this.title = title;

        Id = Interlocked.Increment(ref nextId);
        WindowEventHandler.InitializeStatic();

        if (!WindowInterop.Create(Id, title, width, height, new WindowSettingsRaw(settings)).TryGetValue(
            out InteropHandle<Window> handle, out ResultError error
        ))
        {
            Exception exception = error.ToException();
            error.Dispose();
            throw new PlatformNotSupportedException(exception.Message);
        }

        Handle = handle;
        WindowEventHandler.RegisterWindow(this);

        Input = new WindowInput(this);
        IsFocused = WindowInterop.IsFocused(Handle);
    }

    public Window(string? title = null, uint width = 1280, uint height = 720) : this(
        title, width, height, new WindowSettings()
    ) {
    }

    ~Window() {
        if (Handle == InteropHandle<Window>.Zero)
            return;

        if (!IsDisposed) {
            WindowEventHandler.UnregisterWindow(Id);
            if (!WindowInterop.Dispose(Handle).TryGetValue(out _, out ResultError error))
                error.ThrowAndDispose();
        }

        WindowInterop.Destroy(Handle);
        Application.RaiseWindowClosed();
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

        Disposed?.Invoke(this, new WindowDisposedEventArgs());

        WindowEventHandler.UnregisterWindow(Id);
        if (!WindowInterop.Dispose(Handle).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();

        assignedCamera?.CompareExchangeRenderTarget(null, this);
        Interlocked.Add(ref referenceCount, IReferenceCoutable.DisposeReferenceCount);
        ReferenceCoutable.RcRelease();

        Focused = null;
        Unfocused = null;
        SizeChanged = null;

        Application.RaiseWindowClosed();
    }

    /// <summary>
    /// Resizes this <see cref="Window"/>.
    /// </summary>
    /// <param name="width">New width.</param>
    /// <param name="height">New height.</param>
    public void Resize(uint width, uint height) {
        if (!ReferenceCoutable.TryRcRetain())
            return;
        
        InteropResult<None> result = WindowInterop.SetPosition(Handle, null, new Vector2<uint>(width, height));
        ReferenceCoutable.RcRelease();
        
        if (!result.TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

    internal void ChangeAssignedCamera(SimpleCamera? camera) {
        lock (assignedCameraLocker) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (assignedCamera is not null)
                assignedCamera.RenderTarget = null;
            assignedCamera = camera;
        }
    }

    internal void PoolEvents() {
        unsafe {
            fixed (WindowInputRaw* pointer = &Input.ProcessBeforePoolEvents())
                WindowInterop.PoolEvents(Handle, new InteropHandle<WindowInputRaw>((IntPtr)pointer));
        }
        Input.ProcessAfterPoolEvents();
    }

    internal void RaiseUserClosed() {
        Dispose();
    }

    internal void RaiseFocused() {
        IsFocused = true;
        Focused?.Invoke(this, new FocusedEventArgs());
    }

    internal void RaiseUnfocused() {
        IsFocused = false;
        Unfocused?.Invoke(this, new UnfocusedEventArgs());
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

    bool IReferenceCoutable.TryRcRetain() {
        if (Interlocked.Increment(ref referenceCount) > 0)
            return true;
        Interlocked.Decrement(ref referenceCount);
        return false;
    }

    void IReferenceCoutable.RcRelease() {
        if (
            Interlocked.Decrement(ref referenceCount) != IReferenceCoutable.DisposeReferenceCount ||
            isReleased.Exchange(true)
        ) {
            return;
        }

        GC.SuppressFinalize(this);
        if (Handle != InteropHandle<Window>.Zero) {
            InteropHandle<Window> handle = Handle;
            Handle = InteropHandle<Window>.Zero;
            WindowInterop.Destroy(handle);
        }
    }

}
