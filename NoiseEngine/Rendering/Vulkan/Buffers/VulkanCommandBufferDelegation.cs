using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Serialization;
using System;

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

    public override void AttachCameraWorker(SimpleCamera camera) {
        VulkanSimpleCameraDelegation cameraDelegation = (VulkanSimpleCameraDelegation)camera.Delegation;
        RenderPass renderPass = cameraDelegation.RenderPass;

        if (renderPass is WindowRenderPass window) {
            references.Add(renderPass);
            Swapchain swapchain = window.Swapchain;

            writer.WriteCommand(CommandBufferCommand.AttachCameraWindow);
            writer.WriteIntN(renderPass.Handle.Pointer);
            writer.WriteIntN(swapchain.Handle.Pointer);
        } else if (renderPass is TextureRenderPass texture) {
            references.Add(renderPass);
            Framebuffer framebuffer = texture.Framebuffer;

            writer.WriteCommand(CommandBufferCommand.AttachCameraTexture);
            writer.WriteIntN(framebuffer.Handle.Pointer);
        } else {
            throw new NotImplementedException("Camera render target is not implemented.");
        }

        writer.WriteIntN(cameraDelegation.ClearColor);
    }

}
