using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsReadOnlyBuffer<T> : GraphicsReadOnlyBuffer where T : unmanaged {

    public GraphicsBufferUsage Usage { get; }

    internal InteropHandle<GraphicsReadOnlyBuffer<T>> Handle { get; }
    internal InteropHandle<GraphicsReadOnlyBuffer<T>> InnerHandle { get; }

    internal override InteropHandle<GraphicsReadOnlyBuffer> HandleUniversal =>
        new InteropHandle<GraphicsReadOnlyBuffer>(Handle.Pointer);
    internal override InteropHandle<GraphicsReadOnlyBuffer> InnerHandleUniversal =>
        new InteropHandle<GraphicsReadOnlyBuffer>(InnerHandle.Pointer);

    private protected GraphicsReadOnlyBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count, InteropHandle<GraphicsReadOnlyBuffer<T>> handle,
        InteropHandle<GraphicsReadOnlyBuffer<T>> innerHandle
    ) : base(device, count) {
        Usage = usage;
        Handle = handle;
        InnerHandle = innerHandle;
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
        unsafe {
            return checked(count * (ulong)sizeof(T));
        }
    }

    /// <summary>
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>.
    /// </summary>
    /// <remarks>
    /// This <see cref="GraphicsBuffer{T}"/> must have <see cref="GraphicsBufferUsage.TransferSource"/> flag.
    /// </remarks>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    public void GetData(Span<T> buffer) {
        if ((ulong)buffer.Length > Count)
            throw new ArgumentOutOfRangeException(nameof(buffer));

        if (!Usage.HasFlag(GraphicsBufferUsage.TransferSource)) {
            throw new InvalidOperationException(
                "GraphicsBuffer has not GraphicsBufferUsage.TransferSource flag."
            );
        }

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
