using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanShaderProperty : ShaderProperty {

    public new VulkanCommonShaderDelegation ShaderDelegation =>
        Unsafe.As<VulkanCommonShaderDelegation>(base.ShaderDelegation);

    public DescriptorUpdateTemplateEntry UpdateTemplateEntry =>
        new DescriptorUpdateTemplateEntry(0, 0, 1, DescriptorType.Storage, 0, 0);

    public int UpdateTemplateDataSize {
        get {
            return Type switch {
                ShaderPropertyType.Buffer => Marshal.SizeOf<DescriptorBufferInfo>(),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public VulkanShaderProperty(
        VulkanCommonShaderDelegation shaderDelegation, int index, ShaderPropertyType type, string name
    ) : base(shaderDelegation, index, type, name) {
    }

    public unsafe void WriteUpdateTemplateData(Span<byte> buffer) {
        switch (Type) {
            case ShaderPropertyType.Buffer:
                fixed (byte* pointer = buffer) {
                    Marshal.StructureToPtr(
                        DescriptorBufferInfo.Create(
                            (GraphicsReadOnlyBuffer)(Value ?? throw new NullReferenceException())
                        ), (nint)pointer, false
                    );
                }
                break;
        }
    }

    internal override VulkanShaderProperty Clone(CommonShaderDelegation newShaderDelegation) {
        return new VulkanShaderProperty((VulkanCommonShaderDelegation)newShaderDelegation, Index, Type, Name);
    }

    private protected override void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) {
        ShaderDelegation.SetPropertyAsDirty(this);
    }

}
