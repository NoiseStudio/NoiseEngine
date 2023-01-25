using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Rendering.Vulkan;

internal class GraphicsPipeline : Pipeline {

    public RenderPass RenderPass { get; }

    public GraphicsPipeline(
        RenderPass renderPass, PipelineLayout layout, IReadOnlyList<PipelineShaderStage> stages,
        PipelineCreateFlags flags
    ) : base(layout, stages, CreateHandle(renderPass, layout, stages, flags)) {
        RenderPass = renderPass;
    }

    private static InteropHandle<Pipeline> CreateHandle(
        RenderPass renderPass, PipelineLayout layout, IReadOnlyList<PipelineShaderStage> stages,
        PipelineCreateFlags flags
    ) {
        Span<PipelineShaderStageRaw> rawStages = stackalloc PipelineShaderStageRaw[stages.Count];
        for (int i = 0; i < stages.Count; i++)
            rawStages[i] = new PipelineShaderStageRaw(stages[i]);

        if (!GraphicsPipelineInterop.Create(
            renderPass.Handle, layout.Handle, rawStages, flags, new GraphicsPipelineCreateInfoRaw()
        ).TryGetValue(
            out InteropHandle<Pipeline> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return handle;
    }

}
