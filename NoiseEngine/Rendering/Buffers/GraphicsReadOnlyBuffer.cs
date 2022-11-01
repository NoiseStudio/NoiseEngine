using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsReadOnlyBuffer<T> where T : unmanaged {

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
        if (Handle == InteropHandle<GraphicsReadOnlyBuffer<T>>.Zero)
            return;

        GraphicsBufferInterop.Destroy(Handle.Pointer);
    }

    /// <summary>
    /// Compute size in bytes from <paramref name="count"/>.
    /// </summary>
    /// <param name="count">T object count.</param>
    /// <returns>Size in bytes of <paramref name="count"/>.</returns>
    public static ulong GetSize(ulong count) {
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
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>
    /// without size and start checks.
    /// </summary>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    /// <param name="start">Start index of copy.</param>
    protected abstract void GetDataUnchecked(Span<T> buffer, ulong start);

}
