using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanShaderProperty : ShaderProperty {

    public uint Binding { get; }
    public nuint Offset { get; }

    public new VulkanCommonShaderDelegation ShaderDelegation =>
        Unsafe.As<VulkanCommonShaderDelegation>(base.ShaderDelegation);

    public DescriptorUpdateTemplateEntry UpdateTemplateEntry =>
        new DescriptorUpdateTemplateEntry(Binding, 0, 1, DescriptorType.Storage, Offset, 0);

    public int UpdateTemplateDataSize {
        get {
            return Type switch {
                ShaderPropertyType.Buffer => Marshal.SizeOf<DescriptorBufferInfo>(),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public VulkanShaderProperty(
        VulkanCommonShaderDelegation shaderDelegation, int index, ShaderPropertyType type, string name, uint binding,
        nuint offset
    ) : base(shaderDelegation, index, type, name) {
        Binding = binding;
        Offset = offset;
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
        return new VulkanShaderProperty(
            (VulkanCommonShaderDelegation)newShaderDelegation, Index, Type, Name, Binding, Offset
        );
    }

    private protected override void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) {
        ShaderDelegation.SetPropertyAsDirty(this);
    }

}
