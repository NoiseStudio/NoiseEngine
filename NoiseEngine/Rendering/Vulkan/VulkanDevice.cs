using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanDevice : GraphicsDevice {

    public new VulkanInstance Instance => Unsafe.As<VulkanInstance>(base.Instance);

    public VulkanDevice(VulkanInstance instance, VulkanDeviceValue value) : base(
        instance, value.ToGraphics()
    ) {
        value.Dispose();
    }

    internal override void InternalDispose() {
        VulkanDeviceInterop.Destroy(Handle);
    }

    protected override void InitializeWorker() {
        if (!VulkanDeviceInterop.Initialize(Handle).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

}
