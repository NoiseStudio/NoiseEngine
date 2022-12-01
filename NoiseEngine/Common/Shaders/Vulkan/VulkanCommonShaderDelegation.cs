using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Common.Shaders.Vulkan;

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
        NeslMethod main = shader.ClassData.GetMethod("Main") ?? throw new NullReferenceException();
        SpirVCompilationResult result = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(main, ExecutionModel.GLCompute)
        });

        module = new ShaderModule(Device, result.GetCode());

        ReadOnlySpan<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[] {
            new DescriptorSetLayoutBinding(0, DescriptorType.Storage, 1, ShaderStageFlags.Compute, 0)
        };
        layout = new DescriptorSetLayout(Device, bindings);

        DescriptorSet = new DescriptorSet(layout);

        propertiesToUpdate = new (bool, VulkanShaderProperty)[1];
        VulkanShaderProperty property = new VulkanShaderProperty(this, 0, ShaderPropertyType.Buffer, "buffer");
        Properties.Add(
            shader.ClassData.GetField("buffer")!,
            property
        );
        propertiesToUpdate[0].property = property;

        // Pipelines.
        if (Shader is ComputeShader computeShader) {
            Kernels = new Dictionary<NeslMethod, ComputeKernel>();
            PipelineLayout pipelineLayout = new PipelineLayout(new DescriptorSetLayout[] { layout });

            ComputePipeline pipeline = new ComputePipeline(pipelineLayout, new PipelineShaderStage(
                ShaderStageFlags.Compute, module, main.Guid.ToString()
            ), PipelineCreateFlags.None);
            Kernels.Add(main, new VulkanComputeKernel(main, computeShader, pipeline));
        }
    }

    private VulkanCommonShaderDelegation(
        ICommonShader shader, ShaderModule module, DescriptorSetLayout layout,
        Dictionary<NeslField, ShaderProperty> properties
    ) : base(shader) {
        this.module = module;
        this.layout = layout;

        DescriptorSet = new DescriptorSet(layout);

        propertiesToUpdate = new (bool, VulkanShaderProperty)[properties.Count];
        foreach ((NeslField field, ShaderProperty property) in properties) {
            VulkanShaderProperty n = (VulkanShaderProperty)property.Clone(this);
            this.Properties.Add(field, n);
            propertiesToUpdate[n.Index].property = n;
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

    internal override VulkanCommonShaderDelegation Clone() {
        return new VulkanCommonShaderDelegation(Shader, module, layout, Properties);
    }

}
