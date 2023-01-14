use std::{sync::Arc, ptr};

use ash::vk;

use super::{render_pass::RenderPass, errors::universal::VulkanUniversalError, swapchain_image_view::SwapchainImageView};

pub struct SwapchainFramebuffer<'init> {
    inner: vk::Framebuffer,
    _attachment: Arc<SwapchainImageView<'init>>,
    render_pass: Arc<RenderPass<'init>>
}

impl<'init> SwapchainFramebuffer<'init> {
    pub fn new(
        render_pass: &Arc<RenderPass<'init>>, image_view: &Arc<SwapchainImageView<'init>>
    ) -> Result<Self, VulkanUniversalError> {
        let vk_create_info = vk::FramebufferCreateInfo {
            s_type: vk::StructureType::FRAMEBUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FramebufferCreateFlags::empty(),
            render_pass: render_pass.inner(),
            attachment_count: 1,
            p_attachments: &image_view.inner(),
            width: 1264,
            height: 681,
            layers: 1
        };

        let initialized = render_pass.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_framebuffer(&vk_create_info, None)
        }?;

        Ok(Self { inner, _attachment: image_view.clone(), render_pass: render_pass.clone() })
    }

    pub fn inner(&self) -> vk::Framebuffer {
        self.inner
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
