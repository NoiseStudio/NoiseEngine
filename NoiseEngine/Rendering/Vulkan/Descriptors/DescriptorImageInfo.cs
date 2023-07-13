using NoiseEngine.Interop;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDescriptorImageInfo.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DescriptorImageInfo(
    InteropHandle<TextureSampler> SamplerInner, InteropHandle<VulkanImageView> ViewInner, VulkanImageLayout Layout
) {

    public DescriptorImageInfo(SampledTexture sampled) : this(
        sampled.Sampler.InnerHandle, sampled.Texture.GetVulkanDefaultImageView().InnerHandle,
        VulkanImageLayout.AttachmentOptimal
    ) {
    }

}
