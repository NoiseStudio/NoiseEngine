namespace NoiseEngine.Rendering.Vulkan;

internal record struct PipelineShaderStage(ShaderStageFlags Stage, ShaderModule Module, string Name);
