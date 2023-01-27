using NoiseEngine.Collections;
using NoiseEngine.Common;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Rendering.PushConstants;
using NoiseEngine.Serialization;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan.Buffers;

internal class VulkanCommandBufferDelegation : GraphicsCommandBufferDelegation {

    private Matrix4x4<float> CameraProjectionViewMatrix { get; set; }

    private RenderPass? RenderPass { get; set; }
    private Shader? AttachedShader { get; set; }
    private VulkanCommonShaderDelegation? AttachedCommonShaderDelegation { get; set; }

    public VulkanCommandBufferDelegation(
        GraphicsCommandBuffer commandBuffer, SerializationWriter writer, FastList<object> references,
        FastList<IReferenceCoutable> rcReferences
    ) : base(commandBuffer, writer, references, rcReferences) {
    }

    public override void Clear() {
        RenderPass = null;
        AttachedShader = null;
        AttachedCommonShaderDelegation = null;
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

        CameraProjectionViewMatrix = camera.ProjectionViewMatrix;
    }

    public override void DrawMeshWorker(Mesh mesh, Material material, Matrix4x4<float> transform) {
        AttachShader(material.Shader);

        (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) = mesh.GetBuffers();

        FastList<object> references = this.references;
        references.EnsureCapacity(references.Count + 2);
        references.UnsafeAdd(vertexBuffer);
        references.UnsafeAdd(indexBuffer);

        writer.WriteCommand(CommandBufferCommand.DrawMesh);
        writer.WriteIntN(vertexBuffer.InnerHandleUniversal.Pointer);
        writer.WriteIntN(indexBuffer.InnerHandleUniversal.Pointer);
        writer.WriteUInt32((uint)mesh.IndexFormat);
        writer.WriteUInt32((uint)indexBuffer.Count);

        VulkanCommonShaderDelegation shaderDelegation = AttachedCommonShaderDelegation!;
        if (shaderDelegation.PushConstantDescriptors.Length == 0) {
            writer.WriteUInt32(0);
        } else {
            writer.WriteUInt32(shaderDelegation.PushConstantSize);
            Span<byte> data = stackalloc byte[(int)shaderDelegation.PushConstantSize];

            foreach (PushConstantDescriptor descriptor in shaderDelegation.PushConstantDescriptors) {
                switch (descriptor.Features) {
                    case RenderingFeatures.ObjectToClipPos:
                        Matrix4x4<float> matrix = CameraProjectionViewMatrix * transform;
                        MemoryMarshal.Write(data[descriptor.Offset..], ref matrix);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            writer.WriteBytes(data);
        }
    }

    private void AttachShader(Shader shader) {
        if (AttachedShader == shader)
            return;

        AttachedShader = shader;
        references.Add(shader);
        AttachedCommonShaderDelegation = (VulkanCommonShaderDelegation)shader.Delegation;

        writer.WriteCommand(CommandBufferCommand.AttachShader);
        writer.WriteIntN(RenderPass!.GetPipeline(shader).Handle.Pointer);
    }

}
