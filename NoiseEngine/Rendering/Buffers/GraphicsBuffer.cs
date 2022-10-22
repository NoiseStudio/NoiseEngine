using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsBuffer<T> : GraphicsReadOnlyBuffer<T> where T : unmanaged {

    private protected GraphicsBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count, InteropHandle<GraphicsReadOnlyBuffer<T>> handle
    ) : base(device, usage, count, handle) {
    }

    /// <summary>
    /// Sets data to this <see cref="GraphicsBuffer{T}"/>.
    /// </summary>
    /// <param name="data">New data.</param>
    public void SetData(ReadOnlySpan<T> data) {
        if ((ulong)data.Length > Count)
            throw new ArgumentOutOfRangeException(nameof(data));

        SetDataUnchecked(data, 0);
    }

    /// <summary>
    /// Sets data to this <see cref="GraphicsBuffer{T}"/> without size and start checks.
    /// </summary>
    /// <param name="data">New data.</param>
    /// <param name="start">Start of <paramref name="data"/> entry.</param>
    protected abstract void SetDataUnchecked(ReadOnlySpan<T> data, ulong start);

}
