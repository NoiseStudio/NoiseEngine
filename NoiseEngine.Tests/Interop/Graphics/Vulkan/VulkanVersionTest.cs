using NoiseEngine.Interop;
using NoiseEngine.Interop.Graphics.Vulkan;
using System;

namespace NoiseEngine.Tests.Interop.Graphics.Vulkan;

public partial class VulkanVersionTest {

    [InteropImport("interop_graphics_vulkan_version_test_managed_create")]
    private static partial InteropResult<VulkanVersion> InteropManagedCreate(VulkanVersion version);

    [InteropImport("interop_graphics_vulkan_version_test_unmanaged_create")]
    private static partial InteropResult<VulkanVersion> InteropUnmanagedCreate(int major, int minor, int revision);

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(5, 1022, 784)]
    public void ManagedCreate(int major, int minor, int revision) {
        Version version = new Version(major, minor, 0, revision);
        Assert.Equal(version, InteropManagedCreate(new VulkanVersion(version)).Value.ToVersion());
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(5, 1022, 784)]
    public void UnmanagedCreate(int major, int minor, int revision) {
        Version version = new Version(major, minor, 0, revision);
        Assert.Equal(version, InteropUnmanagedCreate(major, minor, revision).Value.ToVersion());
    }

}
