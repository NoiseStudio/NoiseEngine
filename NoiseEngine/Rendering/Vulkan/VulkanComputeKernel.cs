using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanComputeKernel : ComputeKernel {

    public ComputePipeline Pipeline { get; }

    public VulkanComputeKernel(
        NeslMethod method, ComputeShader shader, ComputePipeline pipeline
    ) : base(method, shader) {
        Pipeline = pipeline;
    }

    internal override VulkanComputeKernel Clone(ComputeShader newShader) {
        return new VulkanComputeKernel(Method, newShader, Pipeline);
    }

}
