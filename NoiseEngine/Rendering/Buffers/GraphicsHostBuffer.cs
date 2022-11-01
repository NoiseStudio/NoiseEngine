using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Interop.Rendering.Vulkan.Buffers;
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
        GC.AddMemoryPressure(GetSizeSigned(Count));
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

    ~GraphicsHostBuffer() {
        if (Handle != InteropHandle<GraphicsReadOnlyBuffer<T>>.Zero)
            return;

        GC.RemoveMemoryPressure(GetSizeSigned(Count));
    }

    private static InteropHandle<GraphicsReadOnlyBuffer<T>> CreateHandle(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong count
    ) {
        device.Initialize();

        Exception? exception = null;

        int i = 0;
        do {
            // Tries to create a graphic buffer.
            IntPtr handle;
            switch (device.Instance.Api) {
                case GraphicsApi.Vulkan:
                    if (!VulkanBufferInterop.Create(device.Handle, usage, GetSize(count), true).TryGetValue(
                        out handle, out ResultError error
                    )) {
                        exception = error.ToException();
                        error.Dispose();
                    }
                    break;
                default:
                    throw new GraphicsApiNotSupportedException(device.Instance.Api);
            }

            if (exception is null)
                return new InteropHandle<GraphicsReadOnlyBuffer<T>>(handle);

            // The first occurrence of GraphicsOutOfMemoryException is ignored, after which memory cleanup is called
            // and then the graphics buffer creating is tried again. Next occurrences will throw exception.
            if (i++ != 0)
                break;
            if (exception is not GraphicsOutOfMemoryException)
                throw exception;

            GraphicsMemoryHelper.WaitToCollect();
        } while(true);

        throw exception;
    }

    private static long GetSizeSigned(ulong count) {
        return (long)Math.Min(GetSize(count), long.MaxValue);
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
