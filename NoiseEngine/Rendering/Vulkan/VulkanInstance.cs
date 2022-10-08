using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using NoiseEngine.Interop.InteropMarshalling;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public VulkanLibrary Library { get; private set; }
    public InteropHandle<VulkanInstance> Handle { get; private set; }

    public VulkanInstance(
        VulkanLibrary library, VulkanLogSeverity? logSeverity = null, VulkanLogType? logType = null
    ) {
        Library = library;

        VulkanInstanceCreateInfo createInfo = new VulkanInstanceCreateInfo(
            new InteropString(Application.Name),
            new VulkanVersion(Application.Version),
            new VulkanVersion(Application.EngineVersion ?? new Version())
        );

        VulkanLogSeverity severity;
        if (logSeverity.HasValue) {
            severity = logSeverity.Value;
        } else {
            severity = VulkanLogSeverity.Warning | VulkanLogSeverity.Error;
#if DEBUG
            severity |= VulkanLogSeverity.Verbose | VulkanLogSeverity.Info;
#endif
        }

        VulkanLogType type;
        if (logType.HasValue) {
            type = logType.Value;

            if (type is VulkanLogType.DeviceAddressBinding) {
                throw new ArgumentException(
                    $"{nameof(VulkanLogType.DeviceAddressBinding)} flag is temporarily unavailable.", nameof(logType)
                );
            }
        } else {
            type = VulkanLogType.General;
#if DEBUG
            type |= VulkanLogType.Validation | VulkanLogType.Performance;
#endif
        }

        InteropResult<InteropHandle<VulkanInstance>> result = VulkanInstanceInterop.Create(
            Library.Handle, createInfo, severity, type
        );
        if (!result.TryGetValue(out InteropHandle<VulkanInstance> nativeInstance, out ResultError error))
            error.ThrowAndDispose();

        Handle = nativeInstance;
        Log.Info($"Created new {this}.");
    }

    public override string ToString() {
        return $"{nameof(VulkanInstance)} {{ Handle = {Handle} }}";
    }

    protected override void ReleaseResources() {
        string toString = ToString();

        InteropHandle<VulkanInstance> handle = Handle;
        Handle = InteropHandle<VulkanInstance>.Zero;
        VulkanInstanceInterop.Destroy(handle);

        Library = null!;

        Log.Info($"Disposed {toString}.");
    }

}
