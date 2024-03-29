﻿using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Rendering.Vulkan.Descriptors;

[Collection(nameof(ApplicationCollection))]
public class DescriptorSetTest : GraphicsTestEnvironment {

    public DescriptorSetTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Vulkan)]
    public unsafe void Update() {
        ReadOnlySpan<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[] {
            new DescriptorSetLayoutBinding(0, DescriptorType.Uniform, 1, ShaderStageFlags.All, 0)
        };
        ReadOnlySpan<DescriptorUpdateTemplateEntry> entries = stackalloc DescriptorUpdateTemplateEntry[] {
            new DescriptorUpdateTemplateEntry(0, 0, 1, DescriptorType.Uniform, 0, 0)
        };

        ReadOnlySpan<byte> data = stackalloc byte[Marshal.SizeOf<DescriptorBufferInfo>()];

        foreach (VulkanDevice device in Fixture.VulkanDevices) {
            DescriptorSetLayout layout = new DescriptorSetLayout(device, bindings);
            DescriptorSet set = new DescriptorSet(layout);
            DescriptorUpdateTemplate template = new DescriptorUpdateTemplate(layout, entries);

            GraphicsHostBuffer<int> buffer = new GraphicsHostBuffer<int>(device, GraphicsBufferUsage.Uniform, 1024);

            fixed (byte* pointer = data)
                Marshal.StructureToPtr(DescriptorBufferInfo.Create(buffer), (nint)pointer, false);

            set.Update(template, data);
        }
    }

}
