using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Vulkan.Buffers;

internal class VulkanCommandBufferDelegation : GraphicsCommandBufferDelegation {

    public VulkanCommandBufferDelegation(
        SerializationWriter writer, FastList<object> references
    ) : base(writer, references) {
    }

    public override void DispatchWorker(ComputeKernel kernel, Vector3<uint> groupCount) {
        VulkanComputeKernel vulkanKernel = (VulkanComputeKernel)kernel;
        VulkanCommonShaderDelegation commonShader = (VulkanCommonShaderDelegation)kernel.Shader.Delegation;

        commonShader.AssertInitialized();

        writer.WriteCommand(CommandBufferCommand.Dispatch);
        writer.WriteIntN(vulkanKernel.Pipeline.Handle.Pointer);
        writer.WriteIntN(commonShader.DescriptorSet.Handle.Pointer);
        writer.WriteUInt32(groupCount.X);
        writer.WriteUInt32(groupCount.Y);
        writer.WriteUInt32(groupCount.Z);
    }

    public override void AttachCameraWorker(Camera camera) {
        VulkanCameraDelegation cameraDelegation = (VulkanCameraDelegation)camera.Delegation;
        RenderPass renderPass = cameraDelegation.RenderPass;
        Framebuffer framebuffer = renderPass.GetFramebuffer();

        FastList<object> references = this.references;
        references.EnsureCapacity(references.Count + 2);
        references.UnsafeAdd(renderPass);
        references.UnsafeAdd(framebuffer);

        writer.WriteCommand(CommandBufferCommand.AttachCamera);
        writer.WriteIntN(renderPass.Handle.Pointer);
        writer.WriteIntN(framebuffer.Handle.Pointer);
    }

}
