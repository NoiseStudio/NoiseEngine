using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanVertexFragmentShaderDelegation : VulkanCommonShaderDelegation {

    public NeslMethod Vertex { get; }
    public NeslMethod Fragment { get; }
    public VertexInputDescription VertexDescription { get; }

    public VulkanVertexFragmentShaderDelegation(ICommonShader shader, ShaderSettings settings) : base(Construct(
        shader, settings, out NeslMethod vertex, out NeslMethod fragment, out SpirVCompilationResult result
    ), result) {
        Vertex = vertex;
        Fragment = fragment;
        VertexDescription = result.VertexInputDesciptions[Vertex];
    }

    private static ICommonShader Construct(
        ICommonShader shader, ShaderSettings settings, out NeslMethod vertex, out NeslMethod fragment,
        out SpirVCompilationResult result
    ) {
        vertex = shader.ClassData.GetMethod("Vertex") ?? throw new NullReferenceException();
        fragment = shader.ClassData.GetMethod("Fragment") ?? throw new NullReferenceException();

        System.IO.File.WriteAllBytes("tak.spv", SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(vertex, ExecutionModel.Vertex),
            //new NeslEntryPoint(Fragment, ExecutionModel.Fragment),
        }, settings).GetCode());

        result = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(vertex, ExecutionModel.Vertex),
            new NeslEntryPoint(fragment, ExecutionModel.Fragment),
        }, settings);

        return shader;
    }

}
