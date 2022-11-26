using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class ComputePipelineTest {

    [FactRequire(TestRequirements.Vulkan)]
    public void Create() {
        /*ReadOnlySpan<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[0];

        foreach (GraphicsDevice d in Application.GraphicsInstance.Devices) {
            if (d is not VulkanDevice device)
                continue;

            DescriptorSetLayout setLayout = new DescriptorSetLayout(device, bindings);
            PipelineLayout layout = new PipelineLayout(new DescriptorSetLayout[] { setLayout });
            ComputePipeline pipeline = new ComputePipeline(layout, default, PipelineCreateFlags.None);
        }*/
    }

}
