using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using NoiseEngine.Interop.InteropMarshalling;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public VulkanLibrary Library { get; private set; }
    public InteropHandle<VulkanInstance> Handle { get; private set; }

    public VulkanInstance(VulkanLibrary library) {
        Library = library;

        VulkanInstanceCreateInfo createInfo = new VulkanInstanceCreateInfo(
            new InteropString(Application.Name),
            new VulkanVersion(Application.Version),
            new VulkanVersion(Application.EngineVersion ?? new Version())
        );

        InteropResult<InteropHandle<VulkanInstance>> result = VulkanInstanceInterop.Create(Library.Handle, createInfo);
        if (!result.TryGetValue(out InteropHandle<VulkanInstance> nativeInstance, out ResultError error))
            error.ThrowAndDispose();

        Handle = nativeInstance;
    }

    protected override void ReleaseResources() {
        InteropHandle<VulkanInstance> handle = Handle;
        Handle = InteropHandle<VulkanInstance>.Zero;
        VulkanInstanceInterop.Destroy(handle);

        Library = null!;
    }

}
