using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System.Collections.Concurrent;

namespace NoiseEngine.Rendering.Vulkan;

internal abstract class RenderPass {

    private readonly ConcurrentDictionary<Shader, GraphicsPipeline> pipelines =
        new ConcurrentDictionary<Shader, GraphicsPipeline>();

    public ICameraRenderTarget RenderTarget { get; }

    internal InteropHandle<RenderPass> Handle { get; }

    protected RenderPass(
        VulkanDevice device, ICameraRenderTarget renderTarget, RenderPassCreateInfo createInfo
    ) {
        RenderTarget = renderTarget;

        if (!RenderPassInterop.Create(device.Handle, createInfo).TryGetValue(
            out InteropHandle<RenderPass> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = handle;
    }

    ~RenderPass() {
        if (Handle == InteropHandle<RenderPass>.Zero)
            return;

        RenderPassInterop.Destroy(Handle);
    }

    public GraphicsPipeline GetPipeline(Shader shader) {
        return pipelines.GetOrAdd(shader, _ => {
            VulkanCommonShaderDelegation shaderDelegation = (VulkanCommonShaderDelegation)shader.Delegation;
            return new GraphicsPipeline(this, shaderDelegation.PipelineLayout, new PipelineShaderStage[] {
                new PipelineShaderStage(
                    ShaderStageFlags.Vertex, shaderDelegation.ModuleVertex, shaderDelegation.Vertex.Guid.ToString()
                ),
                new PipelineShaderStage(
                    ShaderStageFlags.Fragment, shaderDelegation.ModuleFragment, shaderDelegation.Fragment.Guid.ToString()
                )
            }, PipelineCreateFlags.None);
        });
    }

}
