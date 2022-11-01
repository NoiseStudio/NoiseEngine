using NoiseEngine.Interop;
using NoiseEngine.Interop.Exceptions;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanLibrary {

    private const string EntrySymbol = "vkGetInstanceProcAddr";

    private readonly object extensionPropertiesLocker = new object();
    private readonly object layerPropertiesLocker = new object();

    private IntPtr nativeLibraryHandle;

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

    public VulkanLibrary(string? libraryPath = null) {
        if (libraryPath is null) {
            libraryPath = LoadDefaultLibraryPath();
        } else if (!NativeLibrary.TryLoad(libraryPath, out nativeLibraryHandle)) {
            nativeLibraryHandle = IntPtr.Zero;
            GC.SuppressFinalize(this);

            throw new LibraryLoadException($"Unable to load library in {libraryPath} path.");
        }

        if (!NativeLibrary.TryGetExport(nativeLibraryHandle, EntrySymbol, out IntPtr symbol)) {
            NativeLibrary.Free(nativeLibraryHandle);
            nativeLibraryHandle = IntPtr.Zero;
            GC.SuppressFinalize(this);

            throw new LibraryLoadException($"The library under path {libraryPath} is not compatible with Vulkan 1.0.");
        }

        InteropResult<InteropHandle<VulkanLibrary>> result = VulkanLibraryInterop.Create(symbol);
        if (!result.TryGetValue(out InteropHandle<VulkanLibrary> native, out ResultError error)) {
            NativeLibrary.Free(nativeLibraryHandle);
            nativeLibraryHandle = IntPtr.Zero;
            GC.SuppressFinalize(this);

            error.ThrowAndDispose();
        }

        Handle = native;
    }

    ~VulkanLibrary() {
        if (nativeLibraryHandle != IntPtr.Zero)
            NativeLibrary.Free(nativeLibraryHandle);
        if (Handle != InteropHandle<VulkanLibrary>.Zero)
            VulkanLibraryInterop.Destroy(Handle);
    }

    public override string ToString() {
        return $"{nameof(VulkanLibrary)} {{ {nameof(Handle)} = {Handle} }}";
    }

    private string LoadDefaultLibraryPath() {
        string libraryPath;
        if (OperatingSystem.IsWindows())
            libraryPath = "vulkan-1.dll";
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsIOS())
            libraryPath = "libvulkan.dylib";
        else if (OperatingSystem.IsAndroid())
            libraryPath = "libvulkan.so";
        else
            libraryPath = "libvulkan.so.1";

        if (NativeLibrary.TryLoad(libraryPath, out nativeLibraryHandle))
            return libraryPath;

        // Try load SwiftShader.
        if (OperatingSystem.IsWindows()) {
            libraryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "NoiseStudio\\SwiftShader\\vk_swiftshader.dll"
            );
        } else {
            libraryPath = "/etc/vulkan/icd.d/libvk_swiftshader.so";
        }

        if (NativeLibrary.TryLoad(libraryPath, out nativeLibraryHandle))
            return libraryPath;

        nativeLibraryHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);

        throw new LibraryLoadException(
            $"Unable to find default Vulkan library. Try specify path to existing library in " +
            $"{nameof(VulkanLibrary)} constructor."
        );
    }

}
