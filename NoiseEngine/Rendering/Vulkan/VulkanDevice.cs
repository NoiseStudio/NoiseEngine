using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Rendering.Buffers;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanDevice : GraphicsDevice {

    public new VulkanInstance Instance => Unsafe.As<VulkanInstance>(base.Instance);

    public VulkanDevice(VulkanInstance instance, VulkanDeviceValue value) : base(
        instance, value.ToGraphics()
    ) {
        value.Dispose();
    }

    ~VulkanDevice() {
        if (Handle == InteropHandle<GraphicsDevice>.Zero)
            return;

        VulkanDeviceInterop.Destroy(Handle);
    }

    internal override InteropHandle<GraphicsCommandBuffer> CreateCommandBuffer(
        ReadOnlySpan<byte> data, GraphicsCommandBufferUsage usage, bool simultaneousExecute
    ) {
        if (!VulkanDeviceInterop.CreateCommandBuffer(Handle, data, usage, simultaneousExecute).TryGetValue(
            out InteropHandle<GraphicsCommandBuffer> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return handle;
    }

    protected override void InitializeWorker() {
        Span<InteropString> enabledExtensions = stackalloc InteropString[SupportsPresentation ? 1 : 0];
        if (SupportsPresentation)
            enabledExtensions[0] = new InteropString("VK_KHR_swapchain");

        if (!VulkanDeviceInterop.Initialize(Handle, enabledExtensions).TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

}
