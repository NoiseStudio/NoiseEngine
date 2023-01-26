use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, pipeline::Pipeline
    },
    serialization::reader::SerializationReader
};

pub fn draw_mesh(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    let vertex_buffer = data.read_unchecked::<vk::Buffer>();

    unsafe {
        vulkan_device.cmd_bind_vertex_buffers(
            buffer.inner(), 0, &[vertex_buffer], &[0]
        );

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
