using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanVersion {

    private const uint MaxMajor = 127;
    private const uint MaxMinor = 1023;
    private const uint MaxRevision = 4095;

    private readonly uint version;

    public VulkanVersion(Version version) {
        const string ErrorFragment1 = " number is less than zero.";
        const string ErrorFragment2 = " number of given version is greaten than ";
        const string ErrorFragment3 = ". This is incompatible with Vulkan. Read " +
            "https://registry.khronos.org/vulkan/specs/1.3/html/chap31.html#extendingvulkan-coreversions-versionnumbers " +
            "for more informations.";

        if (version.Major < 0)
            throw new ArgumentOutOfRangeException(nameof(version), $"Major{ErrorFragment1}");
        if (version.Minor < 0)
            throw new ArgumentOutOfRangeException(nameof(version), $"Minor{ErrorFragment2}");
        if (version.Revision < 0)
            throw new ArgumentOutOfRangeException(nameof(version), $"Revision{ErrorFragment3}");

        if (version.Major > MaxMajor)
            throw new ArgumentOutOfRangeException(nameof(version), $"Major{ErrorFragment2}{MaxMajor}{ErrorFragment3}");
        if (version.Minor > MaxMinor)
            throw new ArgumentOutOfRangeException(nameof(version), $"Minor{ErrorFragment2}{MaxMinor}{ErrorFragment3}");
        if (version.Revision > MaxRevision) {
            throw new ArgumentOutOfRangeException(nameof(version),
                $"Revision{ErrorFragment2}{MaxRevision}{ErrorFragment3}");
        }

        this.version = ConstructFromVersion(version);
    }

    private VulkanVersion(uint version) {
        this.version = version;
    }

    public static bool TryParse(Version version, out VulkanVersion vulkanVersion) {
        if (
            version.Major < 0 || version.Major > MaxMajor ||
            version.Minor < 0 || version.Minor > MaxMinor ||
            version.Revision < 0 || version.Revision > MaxRevision
        ) {
            vulkanVersion = default;
            return false;
        }

        vulkanVersion = new VulkanVersion(ConstructFromVersion(version));
        return true;
    }

    private static uint ConstructFromVersion(Version version) {
        return (uint)version.Major << 22 | (uint)version.Minor << 12 | (uint)version.Revision;
    }

    public Version ToVersion() {
        return new Version((int)(version >> 22), (int)((version >> 12) & MaxMinor), 0, (int)(version & MaxRevision));
    }

}
