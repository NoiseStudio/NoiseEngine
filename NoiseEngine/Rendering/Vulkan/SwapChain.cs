using NoiseEngine.Common;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Threading;
using System;
using System.Threading;

namespace NoiseEngine.Rendering.Vulkan;

internal class Swapchain : IDisposable, IReferenceCoutable {

    private readonly ManualResetEvent destroyResetEvent = new ManualResetEvent(false);

    private long referenceCount = 1;
    private AtomicBool isReleased;

    public VulkanDevice Device { get; }
    public TextureFormat Format { get; }

    internal InteropHandle<Swapchain> Handle { get; }

    public Swapchain(VulkanDevice device, Window window, uint targetMinImageCount) {
        device.Initialize();
        Device = device;

        if (!SwapchainInterop.Create(device.Handle, window.Handle, targetMinImageCount).TryGetValue(
            out SwapchainCreateReturnValue result, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = result.Handle;
        Format = result.Format;
    }

    ~Swapchain() {
        if (!destroyResetEvent.SafeWaitHandle.IsClosed)
            destroyResetEvent.Dispose();

        if (Handle == InteropHandle<Swapchain>.Zero)
            return;

        SwapchainInterop.Destroy(Handle);
    }

    public void Dispose() {
        Interlocked.Add(ref referenceCount, IReferenceCoutable.DisposeReferenceCount);
        ((IReferenceCoutable)this).RcRelease();

        while (!destroyResetEvent.WaitOne(16)) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        destroyResetEvent.Dispose();
    }

    public uint ChangeMinImageCount(uint targetCount) {
        if (!SwapchainInterop.ChangeMinImageCount(Handle, targetCount).TryGetValue(
            out uint result, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return result;
    }

    bool IReferenceCoutable.TryRcRetain() {
        if (Interlocked.Increment(ref referenceCount) > 0)
            return true;
        Interlocked.Decrement(ref referenceCount);
        return false;
    }

    void IReferenceCoutable.RcRelease() {
        if (
            Interlocked.Decrement(ref referenceCount) != IReferenceCoutable.DisposeReferenceCount ||
            isReleased.Exchange(true)
        ) {
            return;
        }

        GC.SuppressFinalize(this);
        SwapchainInterop.Destroy(Handle);
        destroyResetEvent.Set();
    }

}
