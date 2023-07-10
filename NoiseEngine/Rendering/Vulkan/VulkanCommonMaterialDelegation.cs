using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanCommonMaterialDelegation : CommonMaterialDelegation {

    private readonly (bool isDirty, VulkanMaterialProperty property)[] propertiesToUpdate;
    private readonly object[] valueReferences;
    private readonly object?[] valueReferencesTemp;

    private bool isInitialized;
    private bool isDirty;
    private DescriptorUpdateTemplate? lastUpdateTemplate;
    private int[] lastUpdateIndexes = Array.Empty<int>();

    internal DescriptorSet DescriptorSet { get; }

    public VulkanCommonMaterialDelegation(
        ICommonShader shader, Dictionary<NeslField, MaterialProperty> properties
    ) : base(shader) {
        propertiesToUpdate = new (bool, VulkanMaterialProperty)[properties.Count];
        foreach ((_, MaterialProperty property) in properties)
            propertiesToUpdate[property.Index].property = (VulkanMaterialProperty)property;

        valueReferences = new object[properties.Count];
        valueReferencesTemp = new object[properties.Count];

        VulkanCommonShaderDelegation delegation = (VulkanCommonShaderDelegation)shader.Delegation;
        DescriptorSet = new DescriptorSet(delegation.Layout);
    }

    public void SetPropertyAsDirty(VulkanMaterialProperty property) {
        isDirty = true;
        propertiesToUpdate[property.Index].isDirty = true;
    }

    public void AssertInitialized() {
        Update();

        if (!isInitialized) {
            throw new InvalidOperationException(
                $"{Shader.Type switch {
                    ShaderType.VertexFragment => nameof(Rendering.Shader),
                    ShaderType.Compute => nameof(ComputeShader),
                    _ => throw new NotImplementedException(),
                }} `{Shader.ClassData.FullName}` has not initialized properties."
            );
        }
    }

    public void Update() {
        if (!isDirty && isInitialized)
            return;

        lock (propertiesToUpdate) {
            if (!isDirty && this.isInitialized)
                return;
            isDirty = false;

            // Get dirty properties.
            Span<int> indexes = stackalloc int[propertiesToUpdate.Length];
            int lastIndex = 0;
            int size = 0;
            bool isInitialized = true;

            for (int i = 0; i < propertiesToUpdate.Length; i++) {
                (bool isDirty, VulkanMaterialProperty property) = propertiesToUpdate[i];
                isInitialized &= property.Value is not null;

                if (!isDirty)
                    continue;

                propertiesToUpdate[i].isDirty = false;
                size += property.UpdateTemplateDataSize;
                indexes[lastIndex++] = i;
            }

            this.isInitialized = isInitialized;
            Span<int> comparedIndexes = indexes[..lastIndex];

            // Get update data.
            Span<byte> data = stackalloc byte[size];
            int dataIndex = 0;

            if (lastUpdateTemplate is not null && comparedIndexes.SequenceEqual(lastUpdateIndexes)) {
                for (int i = 0; i < lastIndex; i++) {
                    int index = indexes[i];
                    VulkanMaterialProperty property = propertiesToUpdate[index].property;

                    property.WriteUpdateTemplateData(data[dataIndex..], valueReferencesTemp, index);
                    dataIndex += property.UpdateTemplateDataSize;
                }
            } else {
                // Get update entries and data.
                Span<DescriptorUpdateTemplateEntry> entries = stackalloc DescriptorUpdateTemplateEntry[lastIndex];

                for (int i = 0; i < lastIndex; i++) {
                    int index = indexes[i];
                    VulkanMaterialProperty property = propertiesToUpdate[index].property;

                    entries[i] = property.UpdateTemplateEntry;
                    property.WriteUpdateTemplateData(data[dataIndex..], valueReferencesTemp, index);
                    dataIndex += property.UpdateTemplateDataSize;
                }

                // Create new template.
                lastUpdateTemplate = new DescriptorUpdateTemplate(DescriptorSet.Layout, entries);
                lastUpdateIndexes = comparedIndexes.ToArray();
            }

            // Update.
            DescriptorSet.Update(lastUpdateTemplate, data);
            this.isInitialized = isInitialized;

            for (int i = 0; i < lastIndex; i++) {
                int index = indexes[i];
                valueReferences[index] = valueReferencesTemp[index]!;
                Debug.Assert(valueReferences[index] is not null);
            }
        }
    }

}
