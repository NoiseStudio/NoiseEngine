using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanMaterialProperty : MaterialProperty {

    public uint Binding { get; }
    public nuint Offset { get; }

    public DescriptorUpdateTemplateEntry UpdateTemplateEntry =>
        new DescriptorUpdateTemplateEntry(Binding, 0, 1, DescriptorType.Storage, Offset, 0);

    public int UpdateTemplateDataSize {
        get {
            return Type switch {
                MaterialPropertyType.Buffer => Marshal.SizeOf<DescriptorBufferInfo>(),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private VulkanCommonMaterialDelegation Delegation => Unsafe.As<VulkanCommonMaterialDelegation>(
        Material.Delegation!
    );

    public VulkanMaterialProperty(
        CommonMaterial material, int index, MaterialPropertyType type, string name, uint binding, nuint offset
    ) : base(material, index, type, name) {
        Binding = binding;
        Offset = offset;
    }

    public unsafe void WriteUpdateTemplateData(Span<byte> buffer) {
        switch (Type) {
            case MaterialPropertyType.Buffer:
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

    internal override VulkanMaterialProperty Clone(CommonMaterial newMaterial) {
        return new VulkanMaterialProperty(newMaterial, Index, Type, Name, Binding, Offset);
    }

    private protected override void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) {
        Delegation.SetPropertyAsDirty(this);
    }

}
