using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using NoiseEngine.Interop.InteropMarshalling;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public InteropHandle<VulkanInstance> NativeInstance { get; private set; }

    public VulkanInstance() {
        VulkanInstanceCreateInfo createInfo = new VulkanInstanceCreateInfo(
            new InteropString(Application.Name),
            new VulkanVersion(Application.Version),
            new VulkanVersion(Application.EngineVersion ?? new Version())
        );

        InteropResult<InteropHandle<VulkanInstance>> result = VulkanInstanceInterop.Create(createInfo);
        if (!result.TryGetValue(out InteropHandle<VulkanInstance> nativeInstance, out ResultError error))
            error.ThrowAndDispose();

        NativeInstance = nativeInstance;
    }

    protected override void ReleaseResources() {
        InteropHandle<VulkanInstance> handle = NativeInstance;
        NativeInstance = InteropHandle<VulkanInstance>.Zero;
        VulkanInstanceInterop.Destroy(handle);
    }

}
