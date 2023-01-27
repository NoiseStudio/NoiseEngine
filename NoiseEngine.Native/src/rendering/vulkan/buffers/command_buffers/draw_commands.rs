use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, pipeline::Pipeline, pipeline_layout::PipelineLayout
    },
    serialization::reader::SerializationReader
};

pub fn draw_mesh(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    let vertex_buffer = data.read_unchecked::<vk::Buffer>();
    let index_buffer = data.read_unchecked::<vk::Buffer>();
    let index_format = data.read_unchecked::<vk::IndexType>();
    let index_buffer_count = data.read_unchecked::<u32>();

    let push_constants_size = data.read_unchecked::<u32>();
    let pipeline_layout = data.read_unchecked::<&Arc<PipelineLayout>>();
    let push_constants_data = data.read_bytes_unchecked(push_constants_size as usize);

    unsafe {
        vulkan_device.cmd_bind_vertex_buffers(
            buffer.inner(), 0, &[vertex_buffer], &[0]
        );

        vulkan_device.cmd_bind_index_buffer(
            buffer.inner(), index_buffer, 0, index_format
        );

        vulkan_device.cmd_push_constants(
            buffer.inner(), pipeline_layout.inner(), vk::ShaderStageFlags::VERTEX,
            0, push_constants_data
        );

        vulkan_device.cmd_draw_indexed(
            buffer.inner(), index_buffer_count, 1, 0,
            0, 0
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
