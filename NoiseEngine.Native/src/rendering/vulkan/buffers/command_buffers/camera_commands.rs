use std::{sync::Arc, ptr};

use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer, render_pass::RenderPass, framebuffer::Framebuffer,
        swapchain::{Swapchain, SwapchainPass}, errors::universal::VulkanUniversalError
    },
    serialization::reader::SerializationReader
};

pub struct AttachCameraWindowOutput<'init: 'fam, 'fam> {
    pub pass: Arc<SwapchainPass<'init, 'fam>>,
    pub image_index: u32
}

pub fn attach_camera_window<'init: 'fam, 'fam>(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) -> Result<AttachCameraWindowOutput<'init, 'fam>, VulkanUniversalError> {
    let render_pass = data.read_unchecked::<&Arc<RenderPass>>();
    let swapchain = data.read_unchecked::<&Arc<Swapchain>>();

    let (pass, image_index) =
        swapchain.get_swapchain_pass_and_accquire_next_image(render_pass)?;
    let framebuffer = pass.get_framebuffer(image_index);

    attach_camera_worker(
        data, buffer, vulkan_device, render_pass, framebuffer.inner(),
        framebuffer.extent()
    );

    Ok(AttachCameraWindowOutput { pass, image_index })
}

pub fn attach_camera_texture(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    let framebuffer = data.read_unchecked::<&Framebuffer>();

    attach_camera_worker(
        data, buffer, vulkan_device, framebuffer.render_pass(), framebuffer.inner(),
        framebuffer.extent()
    );
}

pub fn detach_camera(buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    unsafe {
        vulkan_device.cmd_end_render_pass(buffer.inner())
    };
}

fn attach_camera_worker(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device,
    render_pass: &Arc<RenderPass>, framebuffer: vk::Framebuffer, framebuffer_extent: vk::Extent2D
) {
    let clear_color = vk::ClearValue { color: *data.read_unchecked::<&vk::ClearColorValue>() };

    let render_pass_info = vk::RenderPassBeginInfo {
        s_type: vk::StructureType::RENDER_PASS_BEGIN_INFO,
        p_next: ptr::null(),
        render_pass: render_pass.inner(),
        framebuffer,
        render_area: vk::Rect2D {
            offset: vk::Offset2D { x: 0, y: 0 },
            extent: framebuffer_extent
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
