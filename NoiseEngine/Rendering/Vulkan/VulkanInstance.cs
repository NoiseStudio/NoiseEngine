using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public new IReadOnlyList<VulkanDevice> Devices => Unsafe.As<IReadOnlyList<VulkanDevice>>(ProtectedDevices);

    public VulkanLibrary Library { get; private set; }
    public InteropHandle<VulkanInstance> Handle { get; private set; }

    protected override IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    public VulkanInstance(
        VulkanLibrary library, VulkanLogSeverity logSeverity, VulkanLogType logType
    ) {
        if (logType.HasFlag(VulkanLogType.DeviceAddressBinding)) {
            throw new ArgumentException(
                $"{nameof(VulkanLogType.DeviceAddressBinding)} flag is temporarily unavailable.", nameof(logType)
            );
        }

        Library = library;

        VulkanInstanceCreateInfo createInfo = new VulkanInstanceCreateInfo(
            new InteropString(Application.Name),
            new VulkanVersion(Application.Version),
            new VulkanVersion(Application.EngineVersion ?? new Version())
        );

        InteropResult<InteropHandle<VulkanInstance>> result = VulkanInstanceInterop.Create(
            Library.Handle, createInfo, logSeverity, logType
        );
        if (!result.TryGetValue(out InteropHandle<VulkanInstance> nativeInstance, out ResultError error))
            error.ThrowAndDispose();

        Handle = nativeInstance;
        Log.Info($"Created new {this}.");

        if (!VulkanInstanceInterop.GetDevices(Handle).TryGetValue(
            out InteropArray<VulkanDeviceValue> devices, out error
        )) {
            error.ThrowAndDispose();
        }

        ProtectedDevices = devices.Select(x => new VulkanDevice(this, x)).ToArray();
        devices.Dispose();
    }

    public override string ToString() {
        return $"{nameof(VulkanInstance)} {{ {nameof(Handle)} = {Handle} }}";
    }

    protected override void ReleaseResources() {
        string toString = ToString();

        foreach (VulkanDevice physicalDevice in Devices)
            physicalDevice.InternalDispose();

        InteropHandle<VulkanInstance> handle = Handle;
        Handle = InteropHandle<VulkanInstance>.Zero;
        VulkanInstanceInterop.Destroy(handle);

        Library = null!;

        Log.Info($"Disposed {toString}.");
    }

}
