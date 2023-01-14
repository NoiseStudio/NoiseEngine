using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanInstance : GraphicsInstance {

    public new IReadOnlyList<VulkanDevice> Devices => Unsafe.As<IReadOnlyList<VulkanDevice>>(ProtectedDevices);

    public override GraphicsApi Api => GraphicsApi.Vulkan;
    public override bool SupportsPresentation => Library.SupportsPresentation;

    public VulkanLibrary Library { get; }
    public InteropHandle<VulkanInstance> Handle { get; }

    protected override IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    private InteropHandle<VulkanInstance> InnerHandle { get; }

    public VulkanInstance(
        VulkanLibrary library, VulkanLogSeverity logSeverity, VulkanLogType logType, bool validation, bool presentation
    ) : base(presentation) {
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

        Span<InteropString> enabledExtensions = stackalloc InteropString[presentation ? 2 : 0];
        if (presentation) {
            enabledExtensions[0] = new InteropString(VulkanExtensions.Surface);

            enabledExtensions[1] = Window.GetWindowApi() switch {
                WindowApi.WindowsApi => new InteropString(VulkanExtensions.SurfaceWin32),
                _ => throw new NotImplementedException("Presentation is not supported on this device.")
            };
        }

        InteropResult<VulkanInstanceCreateReturnValue> result = VulkanInstanceInterop.Create(
            Library.Handle, createInfo, logSeverity, logType, validation, enabledExtensions
        );

        // Dispose extensions.
        foreach (InteropString extension in enabledExtensions)
            extension.Dispose();

        if (!result.TryGetValue(out VulkanInstanceCreateReturnValue returnValue, out ResultError error))
            error.ThrowAndDispose();

        Handle = returnValue.Handle;
        InnerHandle = returnValue.InnerHandle;
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
    }

    public override string ToString() {
        return $"{nameof(VulkanInstance)} {{ {nameof(InnerHandle)} = {InnerHandle} }}";
    }

}
