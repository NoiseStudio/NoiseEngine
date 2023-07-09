using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanMaterialProperty : MaterialProperty {

    public uint Binding { get; }
    public DescriptorType DescriptorType { get; }
    public nuint Offset { get; }

    public DescriptorUpdateTemplateEntry UpdateTemplateEntry =>
        new DescriptorUpdateTemplateEntry(Binding, 0, 1, DescriptorType, Offset, 0);

    public int UpdateTemplateDataSize {
        get {
            return Type switch {
                MaterialPropertyType.Texture2D => Marshal.SizeOf<DescriptorImageInfo>(),
                MaterialPropertyType.Buffer => Marshal.SizeOf<DescriptorBufferInfo>(),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private VulkanCommonMaterialDelegation Delegation => Unsafe.As<VulkanCommonMaterialDelegation>(
        Material.Delegation!
    );

    public VulkanMaterialProperty(
        CommonMaterial material, int index, MaterialPropertyType type, string name, uint binding,
        DescriptorType descriptorType, nuint offset
    ) : base(material, index, type, name) {
        Binding = binding;
        DescriptorType = descriptorType;
        Offset = offset;
    }

    public unsafe void WriteUpdateTemplateData(Span<byte> buffer, object?[] valueReferences, int index) {
        switch (Type) {
            case MaterialPropertyType.Texture2D:
                SampledTexture sampled = (SampledTexture)(Value ?? throw new NullReferenceException());
                valueReferences[index] = sampled;

                fixed (byte* pointer = buffer)
                    Marshal.StructureToPtr(new DescriptorImageInfo(sampled), (nint)pointer, false);
                break;
            case MaterialPropertyType.Buffer:
                GraphicsReadOnlyBuffer vbuffer = (GraphicsReadOnlyBuffer)(Value ?? throw new NullReferenceException());
                valueReferences[index] = vbuffer;

                fixed (byte* pointer = buffer)
                    Marshal.StructureToPtr(DescriptorBufferInfo.Create(vbuffer), (nint)pointer, false);
                break;
        }
    }

    internal override VulkanMaterialProperty Clone(CommonMaterial newMaterial) {
        return new VulkanMaterialProperty(newMaterial, Index, Type, Name, Binding, DescriptorType, Offset);
    }

    private protected override void SetTexture2DUnchecked(SampledTexture sampled) {
        Delegation.SetPropertyAsDirty(this);
    }

    private protected override void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) {
        Delegation.SetPropertyAsDirty(this);
    }

}
