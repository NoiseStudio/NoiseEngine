using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Threading;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsReadOnlyBuffer<T> : IDisposable where T : unmanaged {

    private AtomicBool isDisposed;

    public GraphicsDevice Device { get; }
    public GraphicsBufferUsage Usage { get; }
    public ulong Count { get; }

    internal InteropHandle<GraphicsReadOnlyBuffer<T>> Handle { get; private set; }

    private protected GraphicsReadOnlyBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count, InteropHandle<GraphicsReadOnlyBuffer<T>> handle
    ) {
        Device = device;
        Usage = usage;
        Count = count;
        Handle = handle;
    }

    ~GraphicsReadOnlyBuffer() {
        //ReleaseResources();
    }

    /// <summary>
    /// Compute size in bytes from <paramref name="count"/>.
    /// </summary>
    /// <param name="count">T object count.</param>
    /// <returns>Size in bytes of <paramref name="count"/>.</returns>
    protected static ulong GetSize(ulong count) {
        return checked(count * (ulong)Marshal.SizeOf<T>());
    }

    /// <summary>
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    public void GetData(Span<T> buffer) {
        if ((ulong)buffer.Length > Count)
            throw new ArgumentOutOfRangeException(nameof(buffer));

        GetDataUnchecked(buffer, 0);
    }

    /// <summary>
    /// Disposes this <see cref="GraphicsReadOnlyBuffer{T}"/>.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>
    /// without size and start checks.
    /// </summary>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    /// <param name="start">Start index of copy.</param>
    protected abstract void GetDataUnchecked(Span<T> buffer, ulong start);

    /// <summary>
    /// Release resources from this <see cref="GraphicsReadOnlyBuffer{T}"/>.
    /// </summary>
    private void ReleaseResources() {
        GraphicsBufferInterop.Destroy(Handle.Pointer);
        Handle = InteropHandle<GraphicsReadOnlyBuffer<T>>.Zero;
    }

}
