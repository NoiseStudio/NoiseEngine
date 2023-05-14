using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Rendering.PushConstants;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanCommonShaderDelegation : CommonShaderDelegation {

    private readonly DescriptorSetLayout layout;

    public VulkanDevice Device => Unsafe.As<VulkanDevice>(Shader.Device);
    public ShaderModule ModuleVertex { get; }
    public ShaderModule ModuleFragment { get; }
    public PipelineLayout PipelineLayout { get; }
    public NeslMethod Vertex { get; }
    public VertexInputDescription VertexDescription { get; }
    public NeslMethod Fragment { get; }
    public PushConstantDescriptor[] PushConstantDescriptors { get; }
    public uint PushConstantSize { get; }

    public VulkanCommonShaderDelegation(ICommonShader shader, ShaderSettings settings) : base(shader) {
        Vertex = shader.ClassData.GetMethod("Vertex") ?? throw new NullReferenceException();
        Fragment = shader.ClassData.GetMethod("Fragment") ?? throw new NullReferenceException();

        SpirVCompilationResult result = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(Vertex, ExecutionModel.Vertex),
            //new NeslEntryPoint(Fragment, ExecutionModel.Fragment),
        }, settings);

        System.IO.File.WriteAllBytes("tak.spv", result.GetCode());

        //ModuleFragment =
        ModuleVertex = new ShaderModule(Device, result.GetCode());
        VertexDescription = result.VertexInputDesciptions[Vertex];
        PushConstantDescriptors = result.PushConstantDescriptors.ToArray();
        PushConstantSize = (uint)PushConstantDescriptors.Sum(x => x.Size);

        result = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(Fragment, ExecutionModel.Fragment),
        }, settings);

        //System.IO.File.WriteAllBytes("tak2.spv", result.GetCode());

        ModuleFragment = new ShaderModule(Device, result.GetCode());

        int i = 0;
        Span<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[result.Bindings.Count];
        foreach ((NeslField field, uint binding) in result.Bindings) {
            bindings[i++] = new DescriptorSetLayoutBinding(
                binding, DescriptorType.Storage, 1, ShaderStageFlags.Vertex | ShaderStageFlags.Fragment, 0
            );
        }

        layout = new DescriptorSetLayout(Device, bindings);

        Span<PushConstantRange> pushConstantRanges = stackalloc PushConstantRange[
            PushConstantDescriptors.Length == 0 ? 0 : 1
        ];
        if (PushConstantDescriptors.Length > 0) {
            PushConstantDescriptor descriptor = PushConstantDescriptors[0];
            switch (descriptor.Features) {
                case RenderingFeatures.ObjectToClipPos:
                    unsafe {
                        pushConstantRanges[0] = new PushConstantRange(
                            ShaderStageFlags.Vertex, 0, (uint)sizeof(Matrix4x4<float>)
                        );
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        PipelineLayout = new PipelineLayout(new DescriptorSetLayout[] { layout }, pushConstantRanges);
    }

}
