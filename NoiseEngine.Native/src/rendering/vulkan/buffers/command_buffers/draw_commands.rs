use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, pipeline::Pipeline
    },
    serialization::reader::SerializationReader
};

pub fn draw_mesh(
    _data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    unsafe {
        vulkan_device.cmd_draw(
            buffer.inner(), 3, 1, 0, 0
        );
    }
}

pub fn attach_shader(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    let pipeline = data.read_unchecked::<&Pipeline>();

    unsafe {
        vulkan_device.cmd_bind_pipeline(
            buffer.inner(), vk::PipelineBindPoint::GRAPHICS,
            pipeline.inner()
        );
    }
}
