use std::{sync::Arc, ptr};

use ash::vk;

use super::{
    render_pass::RenderPass, errors::universal::VulkanUniversalError, swapchain_image_view::SwapchainImageView,
    swapchain::Swapchain
};

pub struct SwapchainFramebuffer<'init> {
    inner: vk::Framebuffer,
    _attachment: Arc<SwapchainImageView<'init>>,
    render_pass: Arc<RenderPass<'init>>,
    extent: vk::Extent2D
}

impl<'init> SwapchainFramebuffer<'init> {
    pub fn new(
        swapchain: &Swapchain, render_pass: &Arc<RenderPass<'init>>, image_view: &Arc<SwapchainImageView<'init>>
    ) -> Result<Self, VulkanUniversalError> {
        let extent = swapchain.extent();

        let vk_create_info = vk::FramebufferCreateInfo {
            s_type: vk::StructureType::FRAMEBUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FramebufferCreateFlags::empty(),
            render_pass: render_pass.inner(),
            attachment_count: 1,
            p_attachments: &image_view.inner(),
            width: extent.width,
            height: extent.height,
            layers: 1
        };

        let initialized = render_pass.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_framebuffer(&vk_create_info, None)
        }?;

        Ok(Self { inner, _attachment: image_view.clone(), render_pass: render_pass.clone(), extent })
    }

    pub fn inner(&self) -> vk::Framebuffer {
        self.inner
    }

    pub fn extent(&self) -> vk::Extent2D {
        self.extent
    }
}

impl Drop for SwapchainFramebuffer<'_> {
    fn drop(&mut self) {
        unsafe {
            self.render_pass.device().initialized().unwrap().vulkan_device().destroy_framebuffer(
                self.inner, None
            );
        }
    }
}
