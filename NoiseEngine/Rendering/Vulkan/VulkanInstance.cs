using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public new IReadOnlyList<VulkanPhysicalDevice> PhysicalDevices =>
        Unsafe.As<IReadOnlyList<VulkanPhysicalDevice>>(ProtectedPhysicalDevices);

    public VulkanLibrary Library { get; private set; }
    public InteropHandle<VulkanInstance> Handle { get; private set; }

    protected override IReadOnlyList<GraphicsPhysicalDevice> ProtectedPhysicalDevices { get; set; }

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

        if (!VulkanInstanceInterop.GetPhysicalDevices(Handle).TryGetValue(
            out InteropArray<VulkanPhysicalDeviceValue> physicalDevices, out error
        )) {
            error.ThrowAndDispose();
        }

        ProtectedPhysicalDevices = physicalDevices.Select(x => new VulkanPhysicalDevice(this, x)).ToArray();
        physicalDevices.Dispose();
    }

    public override string ToString() {
        return $"{nameof(VulkanInstance)} {{ {nameof(Handle)} = {Handle} }}";
    }

    protected override void ReleaseResources() {
        string toString = ToString();

        foreach (VulkanPhysicalDevice physicalDevice in PhysicalDevices)
            physicalDevice.InternalDispose();

        InteropHandle<VulkanInstance> handle = Handle;
        Handle = InteropHandle<VulkanInstance>.Zero;
        VulkanInstanceInterop.Destroy(handle);

        Library = null!;

        Log.Info($"Disposed {toString}.");
    }

}
