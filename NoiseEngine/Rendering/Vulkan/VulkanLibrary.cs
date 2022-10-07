using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanLibrary {

    public InteropHandle<VulkanLibrary> Handle { get; }

    public VulkanLibrary() {
        InteropResult<InteropHandle<VulkanLibrary>> result = VulkanLibraryInterop.Create();
        if (!result.TryGetValue(out InteropHandle<VulkanLibrary> native, out ResultError error))
            error.ThrowAndDispose();

        Handle = native;
    }

    ~VulkanLibrary() {
        VulkanLibraryInterop.Destroy(Handle);
    }

}
