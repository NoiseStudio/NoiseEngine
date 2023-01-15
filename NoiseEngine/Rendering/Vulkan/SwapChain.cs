using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal class Swapchain {

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
        if (Handle == InteropHandle<Swapchain>.Zero)
            return;

        SwapchainInterop.Destroy(Handle);
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
