using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanLibrary {

    private readonly object extensionPropertiesLocker = new object();
    private readonly object layerPropertiesLocker = new object();

    private IReadOnlyList<VulkanExtensionProperties>? extensionProperties;
    private IReadOnlyList<VulkanLayerProperties>? layerProperties;

    public InteropHandle<VulkanLibrary> Handle { get; }

    public IReadOnlyList<VulkanExtensionProperties> ExtensionProperties {
        get {
            if (extensionProperties is not null)
                return extensionProperties;

            lock (extensionPropertiesLocker) {
                if (extensionProperties is not null)
                    return extensionProperties;

                if (!VulkanLibraryInterop.GetExtensionProperties(Handle).TryGetValue(
                    out InteropArray<VulkanExtensionPropertiesRaw> extensions, out ResultError error
                )) {
                    error.ThrowAndDispose();
                }

                extensionProperties = extensions.Select(x => new VulkanExtensionProperties(x)).ToArray();
                extensions.Dispose();

                return extensionProperties;
            }
        }
    }

    public IReadOnlyList<VulkanLayerProperties> LayerProperties {
        get {
            if (layerProperties is not null)
                return layerProperties;

            lock (layerPropertiesLocker) {
                if (layerProperties is not null)
                    return layerProperties;

                if (!VulkanLibraryInterop.GetLayerProperties(Handle).TryGetValue(
                    out InteropArray<VulkanLayerPropertiesRaw> layers, out ResultError error
                )) {
                    error.ThrowAndDispose();
                }

                layerProperties = layers.Select(x => new VulkanLayerProperties(x)).ToArray();
                layers.Dispose();

                return layerProperties;
            }
        }
    }

    public bool SupportsDebugUtils => ExtensionProperties.Any(x => x.Name == VulkanExtensions.DebugUtils);
    public bool SupportsValidationLayers =>
        SupportsDebugUtils && LayerProperties.Any(x => x.Name == VulkanLayers.KhronosValidation);

    public VulkanLibrary() {
        InteropResult<InteropHandle<VulkanLibrary>> result = VulkanLibraryInterop.Create();
        if (!result.TryGetValue(out InteropHandle<VulkanLibrary> native, out ResultError error))
            error.ThrowAndDispose();

        Handle = native;
    }

    ~VulkanLibrary() {
        VulkanLibraryInterop.Destroy(Handle);
    }

    public override string ToString() {
        return $"{nameof(VulkanLibrary)} {{ {nameof(Handle)} = {Handle} }}";
    }

}
