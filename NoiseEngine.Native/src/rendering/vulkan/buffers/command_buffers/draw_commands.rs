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

    let viewport = vk::Viewport {
        x: 0.0,
        y: 0.0,
        width: 1280.0,
        height: 720.0,
        min_depth: 0.0,
        max_depth: 1.0,
    };

    unsafe {
        vulkan_device.cmd_set_viewport(buffer.inner(), 0, &[viewport]);
    }

    let scissor = vk::Rect2D {
        offset: vk::Offset2D { x: 0, y: 0 },
        extent: vk::Extent2D { width: 1280, height: 720 },
    };

    unsafe {
        vulkan_device.cmd_set_scissor(buffer.inner(), 0, &[scissor]);
    }
}
