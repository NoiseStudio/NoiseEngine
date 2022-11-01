using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public new IReadOnlyList<VulkanDevice> Devices => Unsafe.As<IReadOnlyList<VulkanDevice>>(ProtectedDevices);

    public override GraphicsApi Api => GraphicsApi.Vulkan;

    public VulkanLibrary Library { get; }
    public InteropHandle<VulkanInstance> Handle { get; }

    protected override IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    public VulkanInstance(
        VulkanLibrary library, VulkanLogSeverity logSeverity, VulkanLogType logType, bool validation
    ) {
        if (logType.HasFlag(VulkanLogType.DeviceAddressBinding)) {
            throw new ArgumentException(
                $"{nameof(VulkanLogType.DeviceAddressBinding)} flag is temporarily unavailable.", nameof(logType)
            );
        }

        Library = library;

        VulkanApplicationInfo createInfo = new VulkanApplicationInfo(
            new InteropString(Application.Name),
            new VulkanVersion(Application.Version),
            new VulkanVersion(Application.EngineVersion ?? new Version())
        );

        InteropResult<InteropHandle<VulkanInstance>> result = VulkanInstanceInterop.Create(
            Library.Handle, createInfo, logSeverity, logType, validation
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

    ~VulkanInstance() {
        if (Handle == InteropHandle<VulkanInstance>.Zero)
            return;

        VulkanInstanceInterop.Destroy(Handle);
        Log.Info($"Disposed {this}.");
    }

    public override string ToString() {
        return $"{nameof(VulkanInstance)} {{ {nameof(Handle)} = {Handle} }}";
    }

}
