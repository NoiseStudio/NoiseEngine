use std::{sync::Arc, ptr};

use ash::vk;

use super::{
    render_pass::RenderPass, errors::universal::VulkanUniversalError, swapchain_image_view::SwapchainImageView,
    image_view::{VulkanImageView, VulkanImageViewCreateInfo}, image::VulkanImage
};

pub struct SwapchainFramebufferAttachment<'init: 'ma, 'ma>  {
    pub image: Arc<VulkanImage<'init, 'ma>>,
    pub create_info: VulkanImageViewCreateInfo
}

pub struct SwapchainFramebuffer<'init: 'ma, 'ma> {
    inner: vk::Framebuffer,
    _attachment: Arc<SwapchainImageView<'init>>,
    render_pass: Arc<RenderPass<'init>>,
    extent: vk::Extent2D,
    _attachments: Vec<VulkanImageView<'init, 'ma>>
}

impl<'init: 'ma, 'ma> SwapchainFramebuffer<'init, 'ma> {
    pub fn new(
        render_pass: &Arc<RenderPass<'init>>, image_view: &Arc<SwapchainImageView<'init>>, extent: vk::Extent2D,
        attachments: &[SwapchainFramebufferAttachment<'init, 'ma>]
    ) -> Result<Self, VulkanUniversalError> {
        let mut inner_attachments = Vec::with_capacity(attachments.len() + 1);
        let mut constructed_attachments = Vec::with_capacity(attachments.len());

        inner_attachments.push(image_view.inner());

        for attachment in attachments {
            let image_view = VulkanImageView::new(&attachment.image, &attachment.create_info)?;

            inner_attachments.push(image_view.inner());
            constructed_attachments.push(image_view);
        }

        let vk_create_info = vk::FramebufferCreateInfo {
            s_type: vk::StructureType::FRAMEBUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FramebufferCreateFlags::empty(),
            render_pass: render_pass.inner(),
            attachment_count: inner_attachments.len() as u32,
            p_attachments: inner_attachments.as_ptr(),
            width: extent.width,
            height: extent.height,
            layers: 1
        };

        let initialized = render_pass.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_framebuffer(&vk_create_info, None)
        }?;

        Ok(Self {
            inner,
            _attachment: image_view.clone(),
            render_pass: render_pass.clone(),
            extent,
            _attachments: constructed_attachments
        })
    }

    pub fn inner(&self) -> vk::Framebuffer {
        self.inner
    }

    pub fn extent(&self) -> vk::Extent2D {
        self.extent
    }
}

impl Drop for SwapchainFramebuffer<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.render_pass.device().initialized().unwrap().vulkan_device().destroy_framebuffer(
                self.inner, None
            );
        }
    }
}
