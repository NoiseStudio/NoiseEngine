using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsBuffer<T> : GraphicsReadOnlyBuffer<T> where T : unmanaged {

    private protected GraphicsBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count, InteropHandle<GraphicsReadOnlyBuffer<T>> handle,
        InteropHandle<GraphicsReadOnlyBuffer<T>> innerHandle
    ) : base(device, usage, count, handle, innerHandle) {
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="GraphicsBuffer{T}"/>.
    /// </summary>
    /// <param name="data">Data to copy.</param>
    public void SetData(ReadOnlySpan<T> data) {
        if ((ulong)data.Length > Count)
            throw new ArgumentOutOfRangeException(nameof(data));

        if (!Usage.HasFlag(GraphicsBufferUsage.TransferDestination)) {
            throw new InvalidOperationException(
                "GraphicsBuffer has not GraphicsBufferUsage.TransferDestination flag."
            );
        }

        SetDataUnchecked(data, 0);
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="GraphicsBuffer{T}"/> starting with <paramref name="index"/>.
    /// </summary>
    /// <remarks>
    /// This <see cref="GraphicsBuffer{T}"/> must have <see cref="GraphicsBufferUsage.TransferDestination"/> flag.
    /// </remarks>
    /// <param name="data">Data to copy.</param>
    /// <param name="index">Start index of copy.</param>
    public void SetData(ReadOnlySpan<T> data, ulong index) {
        if (index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if ((ulong)data.Length + index > Count)
            throw new ArgumentOutOfRangeException(nameof(data));

        if (!Usage.HasFlag(GraphicsBufferUsage.TransferDestination)) {
            throw new InvalidOperationException(
                "GraphicsBuffer has not GraphicsBufferUsage.TransferDestination flag."
            );
        }

        SetDataUnchecked(data, index);
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="GraphicsBuffer{T}"/> without size and start checks.
    /// </summary>
    /// <param name="data">Data to copy.</param>
    /// <param name="index">Start index of copy.</param>
    protected abstract void SetDataUnchecked(ReadOnlySpan<T> data, ulong index);

}
