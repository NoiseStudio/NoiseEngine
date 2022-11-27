use ash::vk;

use crate::{
    serialization::reader::SerializationReader,
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, pipeline::Pipeline, descriptors::set::DescriptorSet
    }
};

pub fn dispatch(data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    let pipeline = data.read_unchecked::<&Pipeline>();
    let descriptor_set = data.read_unchecked::<&DescriptorSet>();

    unsafe {
        vulkan_device.cmd_bind_pipeline(
            buffer.inner(), vk::PipelineBindPoint::COMPUTE,
            pipeline.inner()
        );

        vulkan_device.cmd_bind_descriptor_sets(
            buffer.inner(), vk::PipelineBindPoint::COMPUTE,
            pipeline.layout().inner(), 0, &[descriptor_set.inner()],
            &[]
        );

        vulkan_device.cmd_dispatch(
            buffer.inner(), data.read_unchecked::<u32>(),
            data.read_unchecked::<u32>(), data.read_unchecked::<u32>()
        );
    }
}
