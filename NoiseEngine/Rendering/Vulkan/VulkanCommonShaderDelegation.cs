using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanCommonShaderDelegation : CommonShaderDelegation {

    private readonly ShaderModule module;
    private readonly DescriptorSetLayout layout;
    private readonly (bool isDirty, VulkanShaderProperty property)[] propertiesToUpdate;

    private bool isInitialized;
    private bool isDirty;
    private DescriptorUpdateTemplate? lastUpdateTemplate;
    private int[] lastUpdateIndexes = Array.Empty<int>();

    public VulkanDevice Device => Unsafe.As<VulkanDevice>(Shader.Device);

    internal DescriptorSet DescriptorSet { get; }

    public VulkanCommonShaderDelegation(ICommonShader shader) : base(shader) {
        NeslMethod[] kernels = shader.ClassData.Methods
            .Where(x => x.Attributes.HasAnyAttribute(nameof(KernelAttribute))).ToArray();

        SpirVCompilationResult result = SpirVCompiler.Compile(kernels
            .Select(x => new NeslEntryPoint(x, ExecutionModel.GLCompute))
        );

        module = new ShaderModule(Device, result.GetCode());

        int i = 0;
        Span<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[result.Bindings.Count];
        foreach ((NeslField field, uint binding) in result.Bindings) {
            bindings[i++] = new DescriptorSetLayoutBinding(
                binding, DescriptorType.Storage, 1, ShaderStageFlags.Compute, 0
            );
        }

        layout = new DescriptorSetLayout(Device, bindings);
        DescriptorSet = new DescriptorSet(layout);

        propertiesToUpdate = new (bool, VulkanShaderProperty)[result.Bindings.Count];
        nuint dataIndex = 0;

        i = 0;
        foreach ((NeslField field, uint binding) in result.Bindings) {
            VulkanShaderProperty property = new VulkanShaderProperty(
                this, i, ShaderPropertyType.Buffer, field.Name, binding, dataIndex
            );

            dataIndex += (nuint)property.UpdateTemplateDataSize;

            Properties.Add(field, property);
            propertiesToUpdate[i++].property = property;
        }

        // Pipelines.
        if (Shader is ComputeShader computeShader) {
            Kernels = new Dictionary<NeslMethod, ComputeKernel>();
            PipelineLayout pipelineLayout = new PipelineLayout(new DescriptorSetLayout[] { layout });

            foreach (NeslMethod kernel in kernels) {
                ComputePipeline pipeline = new ComputePipeline(pipelineLayout, new PipelineShaderStage(
                    ShaderStageFlags.Compute, module, kernel.Guid.ToString()
                ), PipelineCreateFlags.None);
                Kernels.Add(kernel, new VulkanComputeKernel(kernel, computeShader, pipeline));
            }
        }
    }

    private VulkanCommonShaderDelegation(ICommonShader newShader, VulkanCommonShaderDelegation old) : base(newShader) {
        module = old.module;
        layout = old.layout;

        DescriptorSet = new DescriptorSet(layout);

        propertiesToUpdate = new (bool, VulkanShaderProperty)[old.Properties.Count];
        foreach ((NeslField field, ShaderProperty property) in old.Properties) {
            VulkanShaderProperty n = (VulkanShaderProperty)property.Clone(this);
            Properties.Add(field, n);
            propertiesToUpdate[n.Index].property = n;
        }

        if (Shader is ComputeShader computeShader) {
            Kernels = new Dictionary<NeslMethod, ComputeKernel>();
            foreach ((NeslMethod method, ComputeKernel kernel) in old.Kernels!) {
                Kernels.Add(method, kernel.Clone(computeShader));
            }
        }
    }

    public void SetPropertyAsDirty(VulkanShaderProperty property) {
        isDirty = true;
        propertiesToUpdate[property.Index].isDirty = true;
    }

    public void AssertInitialized() {
        Update();

        if (!isInitialized) {
            throw new InvalidOperationException(
                $"{(IsCompute ? "Compute" : "")}Shader `{Shader.ClassData.FullName}` has not initialized properties."
            );
        }
    }

    public void Update() {
        if (!isDirty)
            return;

        lock (propertiesToUpdate) {
            if (!isDirty)
                return;
            isDirty = false;

            // Get dirty properties.
            Span<int> indexes = stackalloc int[propertiesToUpdate.Length];
            int lastIndex = 0;
            int size = 0;
            bool isInitialized = true;

            for (int i = 0; i < propertiesToUpdate.Length; i++) {
                (bool isDirty, VulkanShaderProperty property) = propertiesToUpdate[i];
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

            if (comparedIndexes.SequenceEqual(lastUpdateIndexes)) {
                for (int i = 0; i < lastIndex; i++) {
                    VulkanShaderProperty property = propertiesToUpdate[indexes[i]].property;

                    property.WriteUpdateTemplateData(data[dataIndex..]);
                    dataIndex += property.UpdateTemplateDataSize;
                }
            } else {
                // Get update entries and data.
                Span<DescriptorUpdateTemplateEntry> entries = stackalloc DescriptorUpdateTemplateEntry[lastIndex];

                for (int i = 0; i < lastIndex; i++) {
                    VulkanShaderProperty property = propertiesToUpdate[indexes[i]].property;

                    entries[i] = property.UpdateTemplateEntry;
                    property.WriteUpdateTemplateData(data[dataIndex..]);
                    dataIndex += property.UpdateTemplateDataSize;
                }

                // Create new template.
                lastUpdateTemplate = new DescriptorUpdateTemplate(layout, entries);
                lastUpdateIndexes = comparedIndexes.ToArray();
            }

            // Update.
            DescriptorSet.Update(lastUpdateTemplate!, data);
        }
    }

    internal override VulkanCommonShaderDelegation Clone(ICommonShader newShader) {
        return new VulkanCommonShaderDelegation(newShader, this);
    }

}
