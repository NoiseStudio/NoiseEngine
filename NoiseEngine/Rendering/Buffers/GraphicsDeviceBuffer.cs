using NoiseEngine.Rendering.Buffers.CommandBuffers;
using System;

namespace NoiseEngine.Rendering.Buffers;

/// <summary>
/// Graphics buffer designed for fast work from graphics device (GPU).
/// </summary>
/// <remarks>Memory is reserved on the graphics memory (VRAM).</remarks>
/// <typeparam name="T">Type of the element.</typeparam>
public class GraphicsDeviceBuffer<T> : GraphicsBuffer<T> where T : unmanaged {

    /// <summary>
    /// Creates new <see cref="GraphicsDeviceBuffer{T}"/> on given <paramref name="device"/>.
    /// </summary>
    /// <param name="device">
    /// <see cref="GraphicsDevice"/> associated with the new <see cref="GraphicsDeviceBuffer{T}"/>.
    /// </param>
    /// <param name="usage">Usage of new <see cref="GraphicsBuffer{T}"/>.</param>
    /// <param name="count">Capacity of new <see cref="GraphicsDeviceBuffer{T}"/>.</param>
    public GraphicsDeviceBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count
    ) : base(device, usage, count, GraphicsBufferHelper<T>.CreateHandle(device, usage, GetSize(count), false)) {
    }

    /// <summary>
    /// Creates new <see cref="GraphicsDeviceBuffer{T}"/> on given <paramref name="device"/>.
    /// </summary>
    /// <param name="device">
    /// <see cref="GraphicsDevice"/> associated with the new <see cref="GraphicsDeviceBuffer{T}"/>.
    /// </param>
    /// <param name="usage">Usage of new <see cref="GraphicsBuffer{T}"/>.</param>
    /// <param name="data">Data that is copied to the new <see cref="GraphicsBuffer{T}"/>.</param>
    public GraphicsDeviceBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ReadOnlySpan<T> data
    ) : this(device, usage, (ulong)data.Length) {
        SetDataUnchecked(data, 0);
    }

    /// <summary>
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>
    /// without size and start checks.
    /// </summary>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    /// <param name="index">Start index of copy.</param>
    protected override void GetDataUnchecked(Span<T> buffer, ulong index) {
        GraphicsHostBuffer<T> host = Device.BufferPool.GetOrCreateHost<T>(
            GraphicsBufferUsage.TransferDestination, Count
        );

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(Device, false);
        commandBuffer.CopyUnchecked(this, host, stackalloc BufferCopyRegion[] {
            new BufferCopyRegion(GetSize(index), 0, GetSize(Count))
        });

        commandBuffer.Execute();
        commandBuffer.Clear();

        host.GetData(buffer);
        Device.BufferPool.UnsafeReturnHostToPool(host);
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="GraphicsBuffer{T}"/> without size and start checks.
    /// </summary>
    /// <param name="data">Data to copy.</param>
    /// <param name="index">Start index of copy.</param>
    protected override void SetDataUnchecked(ReadOnlySpan<T> data, ulong index) {
        GraphicsHostBuffer<T> host = Device.BufferPool.GetOrCreateHost<T>(GraphicsBufferUsage.TransferSource, Count);
        host.SetData(data);

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(Device, false);
        commandBuffer.CopyUnchecked(host, this, stackalloc BufferCopyRegion[] {
            new BufferCopyRegion(0, GetSize(index), GetSize(Count))
        });

        commandBuffer.Execute();
        commandBuffer.Clear();

        Device.BufferPool.UnsafeReturnHostToPool(host);
    }

}
