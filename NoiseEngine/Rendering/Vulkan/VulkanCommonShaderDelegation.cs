using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Rendering.PushConstants;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal abstract class VulkanCommonShaderDelegation : CommonShaderDelegation {

    public VulkanDevice Device => Unsafe.As<VulkanDevice>(Shader.Device);
    public ShaderModule Module { get; }
    public PipelineLayout PipelineLayout { get; }
    public DescriptorSetLayout Layout { get; }
    public PushConstantDescriptor[] PushConstantDescriptors { get; }
    public uint PushConstantSize { get; }

    public VulkanCommonShaderDelegation(ICommonShader shader, SpirVCompilationResult result) : base(shader) {
        Module = new ShaderModule(Device, result.GetCode());
        PushConstantDescriptors = result.PushConstantDescriptors.ToArray();
        PushConstantSize = (uint)PushConstantDescriptors.Sum(x => x.Size);

        Span<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[result.Bindings.Count];
        if (result.Bindings.Count > 0) {
            Properties = new (NeslField, MaterialProperty)[result.Bindings.Count];
            int i = 0;
            nuint dataIndex = 0;

            foreach ((NeslField field, uint binding) in result.Bindings) {
                DescriptorType descriptorType = GetDescriptorTypeFromNeslType(field.FieldType);

                // Binding.
                bindings[i] = new DescriptorSetLayoutBinding(
                    binding, descriptorType, 1, shader.Type switch {
                        ShaderType.VertexFragment => ShaderStageFlags.Vertex | ShaderStageFlags.Fragment,
                        ShaderType.Compute => ShaderStageFlags.Compute,
                        _ => throw new NotImplementedException()
                    }, 0
                );

                // Property.
                VulkanMaterialProperty property = new VulkanMaterialProperty(
                    null!, i, GetMaterialPropertyTypeFromNeslType(field.FieldType), field.Name, binding, descriptorType,
                    dataIndex
                );
                dataIndex += (nuint)property.UpdateTemplateDataSize;
                Properties[i++] = (field, property);
            }
        }

        Layout = new DescriptorSetLayout(Device, bindings);

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

        PipelineLayout = new PipelineLayout(new DescriptorSetLayout[] { Layout }, pushConstantRanges);
    }

    private static DescriptorType GetDescriptorTypeFromNeslType(NeslType neslType) {
        return neslType.FullNameWithAssembly switch {
            Nesl.Default.Textures.Texture2DName => DescriptorType.CombinedImageSampler,
            Nesl.Default.Buffers.RwBufferName => DescriptorType.Storage,
            _ => throw new NotImplementedException()
        };
    }

    private static MaterialPropertyType GetMaterialPropertyTypeFromNeslType(NeslType neslType) {
        return neslType.FullNameWithAssembly switch {
            Nesl.Default.Textures.Texture2DName => MaterialPropertyType.Texture2D,
            Nesl.Default.Buffers.RwBufferName => MaterialPropertyType.Buffer,
            _ => throw new NotImplementedException()
        };
    }

}
