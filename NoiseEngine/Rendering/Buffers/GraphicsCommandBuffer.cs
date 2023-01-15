using NoiseEngine.Collections;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan.Buffers;
using NoiseEngine.Serialization;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Buffers;

/// <summary>
/// Command buffer used to execute commands on a <see cref="GraphicsDevice"/>.
/// </summary>
/// <remarks>Must be externally synchronized.</remarks>
public class GraphicsCommandBuffer {

    private readonly GraphicsCommandBufferDelegation delegation;
    private readonly FastList<object> references = new FastList<object>();
    private readonly SerializationWriter writer = new SerializationWriter(BitConverter.IsLittleEndian);
    private readonly FastList<GraphicsFence> fences = new FastList<GraphicsFence>();
    private readonly GCHandle gcHandle;

    private bool simultaneousExecute;
    private int writerCountOnHandleCreation;
    private InteropHandle<GraphicsCommandBuffer> handle;

    private bool graphics;
    private bool computing;
    private bool transfer;

    public GraphicsDevice Device { get; }
    public SimpleCamera? AttachedCamera { get; private set; }

    /// <summary>
    /// Specifies that a <see cref="GraphicsCommandBuffer"/> can be simultaneous executed
    /// and attached into multiple primary <see cref="GraphicsCommandBuffer"/>.
    /// </summary>
    /// <remarks>Changing the value calls the <see cref="Deconstruct"/> method.</remarks>
    public bool SimultaneousExecute {
        get => simultaneousExecute;
        set {
            if (simultaneousExecute != value) {
                simultaneousExecute = value;
                Deconstruct();
            }
        }
    }

    public GraphicsCommandBuffer(GraphicsDevice device, bool simultaneousExecute) {
        device.Initialize();

        Device = device;
        this.simultaneousExecute = simultaneousExecute;

        delegation = device.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanCommandBufferDelegation(writer, references),
            _ => throw new GraphicsApiNotSupportedException(device.Instance.Api),
        };

        // Create GC handle to prevent finalization of references and fences until this command buffer is alive.
        gcHandle = GCHandle.Alloc(new object[] { references, fences });
    }

    ~GraphicsCommandBuffer() {
        if (handle == InteropHandle<GraphicsCommandBuffer>.Zero) {
            gcHandle.Free();
            return;
        }

        if (fences.Any(x => !x.IsSignaled)) {
            GraphicsCommandBufferCleaner.Enqueue(new GraphicsCommandBufferCleanData(
                Device, handle, gcHandle, references, fences
            ));
            return;
        }

        gcHandle.Free();
        references.Clear();
        fences.Clear();
        DestroyHandle(handle);
    }

    internal static void DestroyHandle(InteropHandle<GraphicsCommandBuffer> handle) {
        GraphicsCommandBufferInterop.Destroy(handle);
    }

    private static ArgumentException CreateUsageNotIncludeException(string paramName, GraphicsBufferUsage usage) {
        return new ArgumentException($"Usage of the {paramName} does not include the {usage} flag.", paramName);
    }

    private static ArgumentException CreateInvalidDeviceException(string paramName, string messageBeginning) {
        return new ArgumentException(
            $"{messageBeginning} is from a device other than this {nameof(GraphicsCommandBuffer)}.", paramName
        );
    }

    /// <summary>
    /// Executes this <see cref="GraphicsFence"/>.
    /// </summary>
    /// <remarks>This method also calls <see cref="Construct"/> method.</remarks>
    /// <returns>New <see cref="GraphicsFence"/> associated with this execution.</returns>
    public GraphicsFence Execute() {
        Construct();

        if (!GraphicsCommandBufferInterop.Execute(handle).TryGetValue(
            out InteropHandle<GraphicsFence> fenceHandle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        FastList<GraphicsFence> fences = this.fences;
        if (fences.Count > 0) {
            int i;
            for (i = 0; i < fences.Count && fences[i].IsSignaled; i++) {
            }

            if (i < fences.Count) {
                if (!SimultaneousExecute) {
                    throw new InvalidOperationException(
                        $"This {nameof(GraphicsCommandBuffer)} cannot be executed simultaneous."
                    );
                }

                fences.AsSpan(i).CopyTo(fences.AsSpan());
                fences.RemoveLast(fences.Count - i);
            } else {
                fences.Clear();
            }
        }

        GraphicsFence fence = new GraphicsFence(Device, fenceHandle);
        fences.Add(fence);
        return fence;
    }

    /// <summary>
    /// Waits for pending <see cref="GraphicsFence"/>s, destroys native handle and clears this
    /// <see cref="GraphicsCommandBuffer"/>.
    /// </summary>
    public void Clear() {
        Deconstruct();
        references.Clear();
        writer.Clear();
    }

    /// <summary>
    /// Construct native handle from recorded data.
    /// </summary>
    /// <remarks>
    /// In a situation where the command buffer is already constructed and the amount of recorded data differs,
    /// the <see cref="Deconstruct"/> method is called first.
    /// </remarks>
    public void Construct() {
        if (handle != InteropHandle<GraphicsCommandBuffer>.Zero) {
            if (writerCountOnHandleCreation != writer.Count)
                Deconstruct();
            else
                return;
        }

        writerCountOnHandleCreation = writer.Count;
        handle = Device.CreateCommandBuffer(
            writer.AsSpan(), new GraphicsCommandBufferUsage(graphics, computing, transfer), SimultaneousExecute
        );
    }

    /// <summary>
    /// Waits for pending <see cref="GraphicsFence"/>s and destroys native handle.
    /// </summary>
    public void Deconstruct() {
        if (handle == InteropHandle<GraphicsCommandBuffer>.Zero)
            return;

        GraphicsFence.WaitAll(fences);
        fences.Clear();

        InteropHandle<GraphicsCommandBuffer> copy = handle;
        handle = InteropHandle<GraphicsCommandBuffer>.Zero;
        DestroyHandle(copy);
    }

    /// <summary>
    /// Copies <paramref name="count"/> of data from <paramref name="sourceBuffer"/> to
    /// <paramref name="destinationBuffer"/> starting with a zero index.
    /// </summary>
    /// <typeparam name="T">Type of the element in buffers.</typeparam>
    /// <param name="sourceBuffer">Source buffer with <see cref="GraphicsBufferUsage.TransferSource"/> flag.</param>
    /// <param name="destinationBuffer">
    /// Destination buffer with <see cref="GraphicsBufferUsage.TransferDestination"/> flag.
    /// </param>
    /// <param name="count">Count of copied data.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Source or destination buffer is shorten than given <paramref name="count"/>.
    /// </exception>
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

    /// <summary>
    /// Dispatches <paramref name="kernel"/> with number of local workgroups defined in
    /// <paramref name="groupCount"/>.
    /// </summary>
    /// <param name="kernel"><see cref="ComputeKernel"/> to dispatch.</param>
    /// <param name="groupCount">Number of local workgroups.</param>
    /// <exception cref="ArgumentException">Some unit of <paramref name="groupCount"/> is a zero.</exception>
    public void Dispatch(ComputeKernel kernel, Vector3<uint> groupCount) {
        if (kernel.Device != Device)
            throw CreateInvalidDeviceException(nameof(kernel), "Compute kernel");

        if (groupCount.X == 0 || groupCount.Y == 0 || groupCount.Z == 0)
            throw new ArgumentException("Group count cannot have zero on any unit.", nameof(groupCount));

        DispatchUnchecked(kernel, groupCount);
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

    internal void CopyUnchecked<T>(
        GraphicsReadOnlyBuffer<T> sourceBuffer, Texture destinationTexture,
        ReadOnlySpan<TextureBufferCopyRegion> regions
    ) where T : unmanaged {
        transfer = true;

        FastList<object> references = this.references;
        references.EnsureCapacity(references.Count + 2);
        references.UnsafeAdd(sourceBuffer);
        references.UnsafeAdd(destinationTexture);

        writer.WriteCommand(CommandBufferCommand.CopyBufferToTexture);
        writer.WriteIntN(sourceBuffer.InnerHandle.Pointer);
        writer.WriteIntN(destinationTexture.Handle.Pointer);

        writer.WriteInt32(regions.Length);
        foreach (TextureBufferCopyRegion region in regions)
            region.Write(writer);
    }

    internal void CopyUnchecked<T>(
        Texture sourceTexture, GraphicsBuffer<T> destinationBuffer,
        ReadOnlySpan<TextureBufferCopyRegion> regions
    ) where T : unmanaged {
        transfer = true;

        FastList<object> references = this.references;
        references.EnsureCapacity(references.Count + 2);
        references.UnsafeAdd(sourceTexture);
        references.UnsafeAdd(destinationBuffer);

        writer.WriteCommand(CommandBufferCommand.CopyTextureToBuffer);
        writer.WriteIntN(sourceTexture.Handle.Pointer);
        writer.WriteIntN(destinationBuffer.InnerHandle.Pointer);

        writer.WriteInt32(regions.Length);
        foreach (TextureBufferCopyRegion region in regions)
            region.Write(writer);
    }

    internal void DispatchUnchecked(ComputeKernel kernel, Vector3<uint> groupCount) {
        computing = true;
        references.Add(kernel);
        delegation.DispatchWorker(kernel, groupCount);
    }

    internal void AttachCameraUnchecked(SimpleCamera camera) {
        graphics = true;
        AttachedCamera = camera;
        delegation.AttachCameraWorker(camera);
    }

    internal void DetachCameraUnchecked() {
        AttachedCamera = null;
        writer.WriteCommand(CommandBufferCommand.DetachCamera);
    }

}
