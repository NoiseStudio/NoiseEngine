﻿using NoiseEngine.Common.Shaders.Vulkan;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Vulkan.Buffers;

internal class VulkanCommandBufferDelegation : GraphicsCommandBufferDelegation {

    public VulkanCommandBufferDelegation(SerializationWriter writer) : base(writer) {
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

}