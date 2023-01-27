using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Rendering.Vulkan;

internal class GraphicsPipeline : Pipeline {

    public RenderPass RenderPass { get; }

    public GraphicsPipeline(
        RenderPass renderPass, PipelineLayout layout, IReadOnlyList<PipelineShaderStage> stages,
        PipelineCreateFlags flags, GraphicsPipelineCreateInfo createInfo
    ) : base(layout, stages, CreateHandle(renderPass, layout, stages, flags, createInfo)) {
        RenderPass = renderPass;
    }

    private static InteropHandle<Pipeline> CreateHandle(
        RenderPass renderPass, PipelineLayout layout, IReadOnlyList<PipelineShaderStage> stages,
        PipelineCreateFlags flags, GraphicsPipelineCreateInfo createInfo
    ) {
        Span<PipelineShaderStageRaw> rawStages = stackalloc PipelineShaderStageRaw[stages.Count];
        for (int i = 0; i < stages.Count; i++)
            rawStages[i] = new PipelineShaderStageRaw(stages[i]);

        InteropHandle<Pipeline> handle;
        unsafe {
            fixed (VertexInputBindingDescription* vertexBinding = createInfo.VertexInputBindingDescription) {
                fixed (VertexInputAttributeDescription* vertexAtttribute = createInfo.VertexInputAttributeDescription) {
                    GraphicsPipelineCreateInfoRaw raw = new GraphicsPipelineCreateInfoRaw() {
                        VertexInputBindingDescription = new InteropReadOnlySpan<VertexInputBindingDescription>(
                            vertexBinding, createInfo.VertexInputBindingDescription.Length
                        ),
                        VertexInputAttributeDescription = new InteropReadOnlySpan<VertexInputAttributeDescription>(
                            vertexAtttribute, createInfo.VertexInputAttributeDescription.Length
                        ),
                        PrimitiveTopology = createInfo.PrimitiveTopology
                    };

                    if (!GraphicsPipelineInterop.Create(
                        renderPass.Handle, layout.Handle, rawStages, flags, raw
                    ).TryGetValue(
                        out handle, out ResultError error
                    )) {
                        error.ThrowAndDispose();
                    }
                }
            }
        }

        return handle;
    }

}
