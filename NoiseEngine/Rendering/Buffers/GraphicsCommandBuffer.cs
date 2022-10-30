using NoiseEngine.Collections;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Serialization;
using System;

namespace NoiseEngine.Rendering.Buffers;

public class GraphicsCommandBuffer {

    private readonly FastList<object> references = new FastList<object>();
    private readonly SerializationWriter writer = new SerializationWriter(BitConverter.IsLittleEndian);

    private int writerCountOnHandleCreation;
    private InteropHandle<GraphicsCommandBuffer> handle;

    private bool graphics;
    private bool computing;
    private bool transfer;

    public GraphicsDevice Device { get; }

    public GraphicsCommandBuffer(GraphicsDevice device) {
        device.Initialize();
        Device = device;
    }

    private static ArgumentException CreateUsageNotIncludeException(string paramName, GraphicsBufferUsage usage) {
        return new ArgumentException($"Usage of the {paramName} does not include the {usage} flag.", paramName);
    }

    private static ArgumentException CreateInvalidDeviceException(string paramName, string messageBeginning) {
        return new ArgumentException(
            $"{messageBeginning} is from a device other than this {nameof(GraphicsCommandBuffer)}.", paramName
        );
    }

    public GraphicsFence Execute() {
        if (handle == InteropHandle<GraphicsCommandBuffer>.Zero) {
            writerCountOnHandleCreation = writer.Count;
            handle = Device.CreateCommandBuffer(
                writer.AsSpan(), new GraphicsCommandBufferUsage(graphics, computing, transfer)
            );
        }

        if (!GraphicsCommandBufferInterop.Execute(handle).TryGetValue(
            out InteropHandle<GraphicsFence> fenceHandle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return new GraphicsFence(Device, fenceHandle);
    }

    /// <summary>
    /// Clears this <see cref="GraphicsCommandBuffer"/>.
    /// </summary>
    public void Clear() {
        GraphicsCommandBufferInterop.Destroy(handle);

        writer.Clear();
        references.Clear();
    }

    public void Copy<T>(
        GraphicsReadOnlyBuffer<T> sourceBuffer, GraphicsBuffer<T> destinationBuffer, ulong count
    ) where T : unmanaged {
        const string Shorter = $" is shorter than given {nameof(count)}.";

        if (!sourceBuffer.Usage.HasFlag(GraphicsBufferUsage.TransferSource))
            throw CreateUsageNotIncludeException(nameof(sourceBuffer), GraphicsBufferUsage.TransferSource);
        if (!destinationBuffer.Usage.HasFlag(GraphicsBufferUsage.TransferDestination))
            throw CreateUsageNotIncludeException(nameof(sourceBuffer), GraphicsBufferUsage.TransferDestination);

        if (sourceBuffer.Count < count)
            throw new ArgumentOutOfRangeException(nameof(sourceBuffer), $"Source buffer{Shorter}");
        if (destinationBuffer.Count < count)
            throw new ArgumentOutOfRangeException(nameof(destinationBuffer), $"Destination buffer{Shorter}");

        if (sourceBuffer.Device != Device)
            throw CreateInvalidDeviceException(nameof(sourceBuffer), "Source buffer");
        if (destinationBuffer.Device != Device)
            throw CreateInvalidDeviceException(nameof(sourceBuffer), "Destination buffer");

        CopyUnchecked(sourceBuffer, destinationBuffer, stackalloc BufferCopyRegion[] {
            new BufferCopyRegion(0, 0, GraphicsReadOnlyBuffer<T>.GetSize(count))
        });
    }

    internal void CopyUnchecked<T1, T2>(
        GraphicsReadOnlyBuffer<T1> sourceBuffer, GraphicsBuffer<T2> destinationBuffer,
        ReadOnlySpan<BufferCopyRegion> regions
    ) where T1 : unmanaged where T2 : unmanaged {
        transfer = true;

        FastList<object> references = this.references;
        references.EnsureCapacity(references.Count + 2);
        references.UnsafeAdd(sourceBuffer);
        references.UnsafeAdd(destinationBuffer);

        writer.WriteCommand(CommandBufferCommand.CopyBuffer);
        writer.WriteIntN(sourceBuffer.Handle.Pointer);
        writer.WriteIntN(destinationBuffer.Handle.Pointer);

        writer.WriteInt32(regions.Length);
        foreach (BufferCopyRegion region in regions) {
            writer.WriteUInt64(region.SourceOffset);
            writer.WriteUInt64(region.DestinationOffset);
            writer.WriteUInt64(region.Size);
        }
    }

}
