using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Interop.Rendering.Vulkan.Buffers;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Rendering.Exceptions;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers;

/// <summary>
/// Graphics buffer designed for fast work from host device (CPU).
/// </summary>
/// <remarks>Memory is reserved on the host memory (RAM).</remarks>
/// <typeparam name="T">Type of the element.</typeparam>
public class GraphicsHostBuffer<T> : GraphicsBuffer<T> where T : unmanaged {

    /// <summary>
    /// Creates new <see cref="GraphicsHostBuffer{T}"/> on given <paramref name="device"/>.
    /// </summary>
    /// <param name="device">
    /// <see cref="GraphicsDevice"/> associated with the new <see cref="GraphicsHostBuffer{T}"/>.
    /// </param>
    /// <param name="usage">Usage of new <see cref="GraphicsBuffer{T}"/>.</param>
    /// <param name="count">Capacity of new <see cref="GraphicsHostBuffer{T}"/>.</param>
    public GraphicsHostBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count
    ) : base(device, usage, count, CreateHandle(device, usage, count)) {
    }

    /// <summary>
    /// Creates new <see cref="GraphicsHostBuffer{T}"/> on given <paramref name="device"/>.
    /// </summary>
    /// <param name="device">
    /// <see cref="GraphicsDevice"/> associated with the new <see cref="GraphicsHostBuffer{T}"/>.
    /// </param>
    /// <param name="usage">Usage of new <see cref="GraphicsBuffer{T}"/>.</param>
    /// <param name="data">Data that is copied to the new <see cref="GraphicsBuffer{T}"/>.</param>
    public GraphicsHostBuffer(
        GraphicsDevice device, GraphicsBufferUsage usage, ReadOnlySpan<T> data
    ) : this(device, usage, (ulong)data.Length) {
        SetDataUnchecked(data, 0);
    }

    private static InteropHandle<GraphicsReadOnlyBuffer<T>> CreateHandle(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count
    ) {
        device.Initialize();

        IntPtr handle;
        switch (device.Instance.Api) {
            case GraphicsApi.Vulkan:
                if (!VulkanBufferInterop.Create(device.Handle, usage, GetSize(count), true).TryGetValue(
                    out handle, out ResultError error
                )) {
                    error.ThrowAndDispose();
                }
                break;
            default:
                throw new GraphicsApiNotSupportedException(device.Instance.Api);
        }

        return new InteropHandle<GraphicsReadOnlyBuffer<T>>(handle);
    }

    /// <summary>
    /// Copies data from this <see cref="GraphicsBuffer{T}"/> to given <paramref name="buffer"/>
    /// without size and start checks.
    /// </summary>
    /// <param name="buffer">Buffer for copied data from this <see cref="GraphicsReadOnlyBuffer{T}"/>.</param>
    /// <param name="start">Start index of copy.</param>
    protected override unsafe void GetDataUnchecked(Span<T> buffer, ulong start) {
        Span<byte> b = MemoryMarshal.Cast<T, byte>(buffer);
        if (!GraphicsBufferInterop.HostRead(Handle.Pointer, b, start).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="GraphicsBuffer{T}"/> without size and start checks.
    /// </summary>
    /// <param name="data">Data to copy.</param>
    /// <param name="start">Start index of copy.</param>
    protected override unsafe void SetDataUnchecked(ReadOnlySpan<T> data, ulong start) {
        ReadOnlySpan<byte> b = MemoryMarshal.Cast<T, byte>(data);
        if (!GraphicsBufferInterop.HostWrite(Handle.Pointer, b, start).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

}
