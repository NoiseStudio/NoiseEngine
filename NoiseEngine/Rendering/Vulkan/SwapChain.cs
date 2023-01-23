using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal class Swapchain : IDisposable {

    public VulkanDevice Device { get; }
    public TextureFormat Format { get; }

    internal InteropHandle<Swapchain> Handle { get; private set; }

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
        if (Handle == InteropHandle<Swapchain>.Zero)
            return;

        SwapchainInterop.Destroy(Handle);
    }

    public void Dispose() {
        InteropHandle<Swapchain> handle = Handle;
        if (handle == InteropHandle<Swapchain>.Zero)
            return;

        Handle = InteropHandle<Swapchain>.Zero;
        SwapchainInterop.Destroy(handle);
    }

    public uint ChangeMinImageCount(uint targetCount) {
        if (!SwapchainInterop.ChangeMinImageCount(Handle, targetCount).TryGetValue(
            out uint result, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return result;
    }

}
