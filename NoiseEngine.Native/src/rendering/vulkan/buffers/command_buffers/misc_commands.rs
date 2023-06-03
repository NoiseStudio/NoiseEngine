use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, descriptors::set::DescriptorSet,
        pipeline::Pipeline,
    },
    serialization::reader::SerializationReader,
};

pub fn attach_shader(
    data: &mut SerializationReader,
    buffer: &mut VulkanCommandBuffer,
    vulkan_device: &ash::Device,
) -> (vk::PipelineLayout, vk::PipelineBindPoint) {
    let bind_point = data.read_unchecked::<vk::PipelineBindPoint>();
    let pipeline = data.read_unchecked::<&Pipeline>();

    unsafe {
        vulkan_device.cmd_bind_pipeline(buffer.inner(), bind_point, pipeline.inner());
    }

    (pipeline.layout().inner(), bind_point)
}

pub fn attach_material(
    data: &mut SerializationReader,
    buffer: &mut VulkanCommandBuffer,
    vulkan_device: &ash::Device,
) {
    let descriptor_set = data.read_unchecked::<&DescriptorSet>();
    let pipeline = buffer.attached_pipeline_layout();

    unsafe {
        vulkan_device.cmd_bind_descriptor_sets(
            buffer.inner(),
            pipeline.1,
            pipeline.0,
            0,
            &[descriptor_set.inner()],
            &[],
        );
    }
}
