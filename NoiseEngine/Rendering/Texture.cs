using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class Texture {

    public GraphicsDevice Device { get; }
    public TextureUsage Usage { get; }
    public TextureFormat Format { get; }

    internal abstract Vector3<uint> Extent { get; }
    internal abstract uint SampleCountInternal { get; }

    internal InteropHandle<Texture> Handle { get; }
    internal InteropHandle<Texture> InnerHandle { get; }

    private protected Texture(
        GraphicsDevice device, TextureUsage usage, TextureFormat format, InteropHandle<Texture> handle,
        InteropHandle<Texture> innerHandle
    ) {
        Device = device;
        Usage = usage;
        Format = format;
        Handle = handle;
        InnerHandle = innerHandle;
    }

    ~Texture() {
        if (Handle == InteropHandle<Texture>.Zero)
            return;

        TextureInterop.Destroy(Handle);
    }

    /// <summary>
    /// Copies data from this <see cref="Texture"/> to given <paramref name="buffer"/>.
    /// </summary>
    /// <remarks>This <see cref="Texture"/> must have <see cref="TextureUsage.TransferSource"/> flag.</remarks>
    /// <typeparam name="T">
    /// Type of element in <paramref name="buffer"/>. Must have the same size as this <see cref="Texture"/> pixel.
    /// </typeparam>
    /// <param name="buffer">Buffer for copied data from this <see cref="Texture"/>.</param>
    public void GetPixels<T>(Span<T> buffer) where T : unmanaged {
        if (!Usage.HasFlag(TextureUsage.TransferSource))
            throw new InvalidOperationException("Texture has not TextureUsage.TransferSource flag.");

        GraphicsHostBuffer<T> host = Device.BufferPool.GetOrCreateHost<T>(
            GraphicsBufferUsage.TransferAll, (ulong)buffer.Length
        );

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(Device, false);
        commandBuffer.CopyUnchecked(this, host, stackalloc TextureBufferCopyRegion[] {
            new TextureBufferCopyRegion(0, Vector3<int>.Zero, Extent, TextureAspect.Color, 0, 0, 1)
        });

        commandBuffer.Execute();
        commandBuffer.Clear();

        host.GetData(buffer);
        Device.BufferPool.UnsafeReturnHostToPool(host);
    }

    /// <summary>
    /// Copies <paramref name="data"/> to this <see cref="Texture"/>.
    /// </summary>
    /// <remarks>This <see cref="Texture"/> must have <see cref="TextureUsage.TransferDestination"/> flag.</remarks>
    /// <typeparam name="T">
    /// Type of element in <paramref name="data"/>. Must have the same size as this <see cref="Texture"/> pixel.
    /// </typeparam>
    /// <param name="data">Data to copy.</param>
    public void SetPixels<T>(ReadOnlySpan<T> data) where T : unmanaged {
        if (!Usage.HasFlag(TextureUsage.TransferDestination))
            throw new InvalidOperationException("Texture has not TextureUsage.TransferDestination flag.");

        GraphicsHostBuffer<T> host = Device.BufferPool.GetOrCreateHost<T>(
            GraphicsBufferUsage.TransferAll, (ulong)data.Length
        );
        host.SetData(data);

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(Device, false);
        commandBuffer.CopyUnchecked(host, this, stackalloc TextureBufferCopyRegion[] {
            new TextureBufferCopyRegion(0, Vector3<int>.Zero, Extent, TextureAspect.Color, 0, 0, 1)
        });

        commandBuffer.Execute();
        commandBuffer.Clear();

        Device.BufferPool.UnsafeReturnHostToPool(host);
    }

}
