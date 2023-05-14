using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanComputeShaderDelegation : VulkanCommonShaderDelegation {

    public Dictionary<NeslMethod, ComputeKernel> Kernels { get; } =
        new Dictionary<NeslMethod, ComputeKernel>();

    public VulkanComputeShaderDelegation(ICommonShader shader, ShaderSettings settings) : base(Construct(
        shader, settings, out NeslMethod[] kernels, out SpirVCompilationResult result
    ), result) {
        foreach (NeslMethod kernel in kernels) {
            ComputePipeline pipeline = new ComputePipeline(PipelineLayout, new PipelineShaderStage(
                ShaderStageFlags.Compute, Module, kernel.Guid.ToString()
            ), PipelineCreateFlags.None);
            Kernels.Add(kernel, new VulkanComputeKernel(kernel, (ComputeShader)shader, pipeline));
        }
    }

    private static ICommonShader Construct(
        ICommonShader shader, ShaderSettings settings, out NeslMethod[] kernels, out SpirVCompilationResult result
    ) {
        kernels = shader.ClassData.Methods.Where(x => x.Attributes.HasAnyAttribute(nameof(KernelAttribute))).ToArray();

        result = SpirVCompiler.Compile(
            kernels.Select(x => new NeslEntryPoint(x, ExecutionModel.GLCompute)), settings
        );

        return shader;
    }

}
