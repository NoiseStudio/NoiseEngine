using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class Texture : ICameraRenderTarget {

    public GraphicsDevice Device { get; }
    public TextureFormat Format { get; }

    internal abstract Vector3<uint> Extent { get; }
    internal abstract uint SampleCountInternal { get; }

    internal InteropHandle<Texture> Handle { get; }
    internal InteropHandle<Texture> InnerHandle { get; }

    Vector3<uint> ICameraRenderTarget.Extent => Extent;
    uint ICameraRenderTarget.SampleCount => SampleCountInternal;

    private protected Texture(
        GraphicsDevice device, TextureFormat format, InteropHandle<Texture> handle, InteropHandle<Texture> innerHandle
    ) {
        Device = device;
        Format = format;
        Handle = handle;
        InnerHandle = innerHandle;
    }

    ~Texture() {
        if (Handle == InteropHandle<Texture>.Zero)
            return;

        TextureInterop.Destroy(Handle);
    }

    public void GetPixels<T>(Span<T> buffer) where T : unmanaged {
        GraphicsHostBuffer<T> host = Device.BufferPool.GetOrCreateHost<T>(
            GraphicsBufferUsage.TransferDestination, (ulong)buffer.Length
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

}
