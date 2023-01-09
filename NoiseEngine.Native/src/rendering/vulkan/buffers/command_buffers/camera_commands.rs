use std::{sync::Arc, ptr};

use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, render_pass::RenderPass, framebuffer::Framebuffer
    },
    serialization::reader::SerializationReader
};

pub fn attach_camera(data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    let render_pass = data.read_unchecked::<&Arc<RenderPass>>();
    let framebuffer = data.read_unchecked::<&Framebuffer>();

    let clear_color = vk::ClearValue { color: *data.read_unchecked::<&vk::ClearColorValue>() };

    let render_pass_info = vk::RenderPassBeginInfo {
        s_type: vk::StructureType::RENDER_PASS_BEGIN_INFO,
        p_next: ptr::null(),
        render_pass: render_pass.inner(),
        framebuffer: framebuffer.inner(),
        render_area: vk::Rect2D {
            offset: vk::Offset2D { x: 0, y: 0 },
            extent: framebuffer.extent()
        },
        clear_value_count: 1,
        p_clear_values: &clear_color,
    };

    unsafe {
        vulkan_device.cmd_begin_render_pass(
            buffer.inner(), &render_pass_info, vk::SubpassContents::INLINE
        )
    };
}

pub fn detach_camera(buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    unsafe {
        vulkan_device.cmd_end_render_pass(buffer.inner())
    };
}
