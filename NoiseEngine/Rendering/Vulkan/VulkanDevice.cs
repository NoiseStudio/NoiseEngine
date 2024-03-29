﻿using NoiseEngine.Interop;
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
        bool activatePresentation = Instance.PresentationEnabled && SupportsPresentation;
        Span<InteropString> enabledExtensions = stackalloc InteropString[activatePresentation ? 1 : 0];
        if (activatePresentation)
            enabledExtensions[0] = new InteropString("VK_KHR_swapchain");

        InteropResult<None> result = VulkanDeviceInterop.Initialize(Handle, enabledExtensions);

        // Dispose extensions.
        foreach (InteropString extension in enabledExtensions)
            extension.Dispose();

        if (!result.TryGetValue(out _, out ResultError error))
            error.ThrowAndDispose();
    }

}
