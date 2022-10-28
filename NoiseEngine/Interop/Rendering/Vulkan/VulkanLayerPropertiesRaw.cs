using NoiseEngine.Rendering.Vulkan;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal struct VulkanLayerPropertiesRaw {

    private unsafe fixed byte name[VulkanConstants.MaxExtensionNameSize];

    public VulkanVersion SpecificationVersion { get; }
    public VulkanVersion ImplementationVersion { get; }

    private unsafe fixed byte description[VulkanConstants.MaxExtensionNameSize];

    public unsafe string Name {
        get {
            fixed (byte* ptr = name)
                return Marshal.PtrToStringUTF8(new IntPtr(ptr)) ?? throw new NullReferenceException();
        }
    }

    public unsafe string Description {
        get {
            fixed (byte* ptr = description)
                return Marshal.PtrToStringUTF8(new IntPtr(ptr)) ?? throw new NullReferenceException();
        }
    }

}
