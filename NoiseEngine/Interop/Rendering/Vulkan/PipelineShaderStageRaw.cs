using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal record struct PipelineShaderStageRaw(
    ShaderStageFlags Stage, InteropHandle<ShaderModule> Module, InteropString Name
) : IDisposable {

    public PipelineShaderStageRaw(PipelineShaderStage stage)
        : this(stage.Stage, stage.Module.Handle, new InteropString(stage.Name)) {
    }

    public void Dispose() {
        Name.Dispose();
    }

}
