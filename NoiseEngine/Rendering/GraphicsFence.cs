using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Rendering;

public class GraphicsFence {

    private readonly InteropHandle<GraphicsFence> handle;

    public GraphicsDevice Device { get; }

    public bool IsSignaled {
        get {
            if (!GraphicsFenceInterop.IsSignaled(handle).TryGetValue(out InteropBool value, out ResultError error))
                error.ThrowAndDispose();
            return value;
        }
    }

    internal GraphicsFence(GraphicsDevice device, InteropHandle<GraphicsFence> handle) {
        Device = device;
        this.handle = handle;
    }

    ~GraphicsFence() {
        GraphicsFenceInterop.Destroy(handle);
    }

    /// <summary>
    /// Waits for all <paramref name="fences"/> to become signaled or the timeout will be exceeded.
    /// </summary>
    /// <param name="fences">
    /// <see cref="GraphicsFence"/> to wait. They must belong to the same <see cref="GraphicsDevice"/>.
    /// </param>
    /// <param name="nanosecondsTimeout">
    /// The timeout period in units of nanoseconds. Timeout is adjusted to the closest value allowed by the
    /// implementation-dependent timeout accuracy, which may be substantially longer than one nanosecond,
    /// and may be longer than the requested period.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when all <paramref name="fences"/> have received a signal;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool WaitAll(IEnumerable<GraphicsFence> fences, ulong nanosecondsTimeout) {
        return WaitMultiple(fences, true, nanosecondsTimeout);
    }

    /// <summary>
    /// Waits for all <paramref name="fences"/> to become signaled.
    /// </summary>
    /// <param name="fences">
    /// <see cref="GraphicsFence"/> to wait. They must belong to the same <see cref="GraphicsDevice"/>.
    /// </param>
    public static void WaitAll(IEnumerable<GraphicsFence> fences) {
        WaitAll(fences, ulong.MaxValue);
    }

    /// <summary>
    /// Waits until at least one of the <paramref name="fences"/> becomes signaled or the timeout will be exceeded.
    /// </summary>
    /// <param name="fences">
    /// <see cref="GraphicsFence"/> to wait. They must belong to the same <see cref="GraphicsDevice"/>.
    /// </param>
    /// <param name="nanosecondsTimeout">
    /// The timeout period in units of nanoseconds. Timeout is adjusted to the closest value allowed by the
    /// implementation-dependent timeout accuracy, which may be substantially longer than one nanosecond,
    /// and may be longer than the requested period.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when least one of the <paramref name="fences"/> has received a signal;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool WaitAny(IEnumerable<GraphicsFence> fences, ulong nanosecondsTimeout) {
        return WaitMultiple(fences, false, nanosecondsTimeout);
    }

    /// <summary>
    /// Waits until at least one of the <paramref name="fences"/> becomes signaled.
    /// </summary>
    /// <param name="fences">
    /// <see cref="GraphicsFence"/> to wait. They must belong to the same <see cref="GraphicsDevice"/>.
    /// </param>
    public static void WaitAny(IEnumerable<GraphicsFence> fences) {
        WaitAny(fences, ulong.MaxValue);
    }

    private static bool WaitMultiple(IEnumerable<GraphicsFence> fences, bool waitAll, ulong nanosecondsTimeout) {
        GraphicsFence[] fencesArray = fences.ToArray();

        int count = fencesArray.Length;
        if (count == 0)
            return true;

        Span<InteropHandle<GraphicsFence>> handles = count <= 1024 ?
            stackalloc InteropHandle<GraphicsFence>[count] : new InteropHandle<GraphicsFence>[count];

        GraphicsDevice device = fencesArray[0].Device;

        int i = 0;
        foreach (GraphicsFence fence in fencesArray) {
            if (device != fence.Device)
                throw new ArgumentException($"Fences are not from the same {nameof(GraphicsDevice)}.", nameof(fences));

            handles[i++] = fence.handle;
        }

        if (!GraphicsFenceInterop.WaitMultiple(handles, waitAll, nanosecondsTimeout).TryGetValue(
            out InteropBool isSignaled, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return isSignaled;
    }

    /// <summary>
    /// Waits for this <see cref="GraphicsFence"/> to become signaled or the timeout will be exceeded.
    /// </summary>
    /// <param name="nanosecondsTimeout">
    /// The timeout period in units of nanoseconds. Timeout is adjusted to the closest value allowed by the
    /// implementation-dependent timeout accuracy, which may be substantially longer than one nanosecond,
    /// and may be longer than the requested period.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this <see cref="GraphicsFence"/> has received a signal;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Wait(ulong nanosecondsTimeout) {
        if (!GraphicsFenceInterop.Wait(handle, nanosecondsTimeout).TryGetValue(
            out InteropBool isSignaled, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return isSignaled;
    }

    /// <summary>
    /// Waits for this <see cref="GraphicsFence"/> to become signaled.
    /// </summary>
    public void Wait() {
        Wait(ulong.MaxValue);
    }

}
