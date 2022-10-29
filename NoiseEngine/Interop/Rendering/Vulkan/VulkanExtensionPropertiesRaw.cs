using NoiseEngine.Rendering.Vulkan;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal struct VulkanExtensionPropertiesRaw {

    private unsafe fixed byte name[VulkanConstants.MaxExtensionNameSize];

    public uint SpecificationVersion { get; }

    public unsafe string Name {
        get {
            fixed (byte* ptr = name)
                return Marshal.PtrToStringUTF8(new IntPtr(ptr)) ?? throw new NullReferenceException();
        }
    }

}
