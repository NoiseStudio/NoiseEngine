using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Presentation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Presentation;

internal static class WindowEventHandler {

    private static readonly ConcurrentDictionary<ulong, WeakReference<Window>> windows =
        new ConcurrentDictionary<ulong, WeakReference<Window>>();

    private static readonly WindowEventHandlerRaw raw;

    public static IEnumerable<Window> Windows {
        get {
            foreach (WeakReference<Window> weak in windows.Values) {
                if (weak.TryGetTarget(out Window? window))
                    yield return window;
            }
        }
    }

    [UnmanagedFunctionPointer(InteropConstants.CallingConvention)]
    public delegate void UserClosedDelegate(ulong id);

    [UnmanagedFunctionPointer(InteropConstants.CallingConvention)]
    public delegate void SizeChangedDelegate(ulong id, uint newWidth, uint newHeight);

    static WindowEventHandler() {
        // Prevents GC cleanup (https://stackoverflow.com/a/43227979/14677292)
        raw = new WindowEventHandlerRaw(UserClosedImpl, SizeChangedImpl);

        if (!WindowEventHandlerInterop.Initialize(raw).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

    public static void InitializeStatic() {
    }

    public static void RegisterWindow(Window window) {
        windows.TryAdd(window.Id, new WeakReference<Window>(window));
    }

    public static void UnregisterWindow(ulong id) {
        windows.TryRemove(id, out _);
    }

    private static bool TryGetWindow(ulong id, [NotNullWhen(true)] out Window? window) {
        if (
            !windows.TryGetValue(id, out WeakReference<Window>? weak) ||
            !weak.TryGetTarget(out Window? reference) ||
            reference.IsDisposed
        ) {
            window = null;
            return false;
        }

        window = reference;
        return true;
    }

    private static void UserClosedImpl(ulong id) {
        if (TryGetWindow(id, out Window? window))
            window.RaiseUserClosed();
    }

    private static void SizeChangedImpl(ulong id, uint newWidth, uint newHeight) {
        if (TryGetWindow(id, out Window? window))
            window.RaiseSizeChanged(newWidth, newHeight);
    }

}
