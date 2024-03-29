use std::{ptr, sync::Arc};

use ash::vk;

use crate::{
    rendering::vulkan::{
        buffers::command_buffer::VulkanCommandBuffer,
        errors::universal::VulkanUniversalError,
        fence::VulkanFence,
        framebuffer::Framebuffer,
        render_pass::RenderPass,
        swapchain::{Swapchain, SwapchainPass},
        synchronized_fence::VulkanSynchronizedFence,
    },
    serialization::reader::SerializationReader,
};

pub struct AttachCameraWindowOutput<'init: 'fam, 'fam> {
    pub pass: Arc<SwapchainPass<'init, 'fam>>,
    pub synchronized_fence: Arc<VulkanSynchronizedFence<'init>>,
    pub frame_index: usize,
    pub image_index: u32,
}

pub fn attach_camera_window<'init: 'fam, 'fam>(
    data: &mut SerializationReader,
    buffer: &VulkanCommandBuffer,
    vulkan_device: &ash::Device,
    used_fence: &Arc<VulkanFence<'init>>,
) -> Result<AttachCameraWindowOutput<'init, 'fam>, VulkanUniversalError> {
    let render_pass = data.read_unchecked::<&Arc<RenderPass>>();
    let swapchain = data.read_unchecked::<&Arc<Swapchain>>();

    let (pass, synchronized_fence, frame_index, image_index) =
        swapchain.get_swapchain_pass_and_accquire_next_image(render_pass, used_fence)?;
    let framebuffer = pass.get_framebuffer(image_index);

    attach_camera_worker(
        data,
        buffer,
        vulkan_device,
        render_pass,
        framebuffer.inner(),
        framebuffer.extent(),
    );

    Ok(AttachCameraWindowOutput {
        pass,
        synchronized_fence,
        frame_index,
        image_index,
    })
}

pub fn attach_camera_texture(
    data: &mut SerializationReader,
    buffer: &VulkanCommandBuffer,
    vulkan_device: &ash::Device,
) {
    let framebuffer = data.read_unchecked::<&Framebuffer>();

    attach_camera_worker(
        data,
        buffer,
        vulkan_device,
        framebuffer.render_pass(),
        framebuffer.inner(),
        framebuffer.extent(),
    );
}

pub fn detach_camera(buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    unsafe { vulkan_device.cmd_end_render_pass(buffer.inner()) };
}

fn attach_camera_worker(
    data: &mut SerializationReader,
    buffer: &VulkanCommandBuffer,
    vulkan_device: &ash::Device,
    render_pass: &Arc<RenderPass>,
    framebuffer: vk::Framebuffer,
    framebuffer_extent: vk::Extent2D,
) {
    let clear_color = vk::ClearValue {
        color: *data.read_unchecked::<&vk::ClearColorValue>(),
    };

    let depth_stencil_clear = vk::ClearValue {
        depth_stencil: vk::ClearDepthStencilValue {
            depth: 1.0,
            stencil: 0,
        },
    };

    let p_clear_values = [clear_color, depth_stencil_clear];
    let render_pass_info = vk::RenderPassBeginInfo {
        s_type: vk::StructureType::RENDER_PASS_BEGIN_INFO,
        p_next: ptr::null(),
        render_pass: render_pass.inner(),
        framebuffer,
        render_area: vk::Rect2D {
            offset: vk::Offset2D { x: 0, y: 0 },
            extent: framebuffer_extent,
        },
        clear_value_count: match render_pass.depth_testing() {
            true => 2,
            false => 1,
        },
        p_clear_values: p_clear_values.as_ptr(),
    };

    unsafe {
        vulkan_device.cmd_begin_render_pass(
            buffer.inner(),
            &render_pass_info,
            vk::SubpassContents::INLINE,
        )
    };

    let viewport = vk::Viewport {
        x: 0.0,
        y: 0.0,
        width: framebuffer_extent.width as f32,
        height: framebuffer_extent.height as f32,
        min_depth: 0.0,
        max_depth: 1.0,
    };

    unsafe {
        vulkan_device.cmd_set_viewport(buffer.inner(), 0, &[viewport]);
    }

    let scissor = vk::Rect2D {
        offset: vk::Offset2D { x: 0, y: 0 },
        extent: framebuffer_extent,
    };

    unsafe {
        vulkan_device.cmd_set_scissor(buffer.inner(), 0, &[scissor]);
    }
}
