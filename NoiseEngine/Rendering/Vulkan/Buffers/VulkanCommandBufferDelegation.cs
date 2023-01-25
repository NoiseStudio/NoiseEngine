using NoiseEngine.Collections;
using NoiseEngine.Common;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Serialization;
using System;

namespace NoiseEngine.Rendering.Vulkan.Buffers;

internal class VulkanCommandBufferDelegation : GraphicsCommandBufferDelegation {

    private RenderPass? RenderPass { get; set; }
    private Shader? AttachedShader { get; set; }

    public VulkanCommandBufferDelegation(
        SerializationWriter writer, FastList<object> references, FastList<IReferenceCoutable> rcReferences
    ) : base(writer, references, rcReferences) {
    }

    public override void Clear() {
        RenderPass = null;
        AttachedShader = null;
    }

    public override void DispatchWorker(ComputeKernel kernel, Vector3<uint> groupCount) {
        VulkanComputeKernel vulkanKernel = (VulkanComputeKernel)kernel;
        VulkanCommonShaderDelegationOld commonShader = (VulkanCommonShaderDelegationOld)kernel.Shader.Delegation;

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
        RenderPass = cameraDelegation.RenderPass;

        if (RenderPass is WindowRenderPass window) {
            FastList<IReferenceCoutable> rcReferences = this.rcReferences;
            rcReferences.EnsureCapacity(rcReferences.Count + 2);

            IReferenceCoutable rcReference = (IReferenceCoutable)window.RenderTarget;
            rcReference.RcRetain();
            rcReferences.UnsafeAdd(rcReference);

            rcReference = window.Swapchain;
            rcReference.RcRetain();
            rcReferences.UnsafeAdd(rcReference);

            references.Add(RenderPass);
            Swapchain swapchain = window.Swapchain;

            writer.WriteCommand(CommandBufferCommand.AttachCameraWindow);
            writer.WriteIntN(RenderPass.Handle.Pointer);
            writer.WriteIntN(swapchain.Handle.Pointer);
        } else if (RenderPass is TextureRenderPass texture) {
            references.Add(RenderPass);
            Framebuffer framebuffer = texture.Framebuffer;

            writer.WriteCommand(CommandBufferCommand.AttachCameraTexture);
            writer.WriteIntN(framebuffer.Handle.Pointer);
        } else {
            throw new NotImplementedException("Camera render target is not implemented.");
        }

        writer.WriteIntN(cameraDelegation.ClearColor);
    }

    public override void DrawMeshWorker(Mesh mesh, Material material) {
        AttachShader(material.Shader);
        writer.WriteCommand(CommandBufferCommand.DrawMesh);
    }

    private void AttachShader(Shader shader) {
        if (AttachedShader == shader)
            return;

        AttachedShader = shader;
        references.Add(shader);

        writer.WriteCommand(CommandBufferCommand.AttachShader);
        writer.WriteIntN(RenderPass!.GetPipeline(shader).Handle.Pointer);
    }

}
