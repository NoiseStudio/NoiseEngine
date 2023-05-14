using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanComputeKernel : ComputeKernel {

    public ComputePipeline Pipeline { get; }

    public VulkanComputeKernel(
        NeslMethod method, ComputeShader shader, ComputePipeline pipeline
    ) : base(method, shader) {
        Pipeline = pipeline;
    }

}
