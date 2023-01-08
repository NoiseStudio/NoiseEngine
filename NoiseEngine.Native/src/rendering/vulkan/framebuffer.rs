use std::{sync::Arc, ptr};

use ash::vk;

use super::{
    image_view::{VulkanImageView, VulkanImageViewCreateInfo}, render_pass::RenderPass,
    errors::universal::VulkanUniversalError, image::VulkanImage
};

#[repr(C)]
pub struct FramebufferAttachment<'init: 'ma, 'ma>  {
    image: &'init Arc<VulkanImage<'init, 'ma>>,
    create_info: VulkanImageViewCreateInfo
}

pub struct Framebuffer<'init: 'ma, 'ma> {
    inner: vk::Framebuffer,
    extent: vk::Extent2D,
    render_pass: Arc<RenderPass<'init>>,
    _attachments: Vec<VulkanImageView<'init, 'ma>>
}

impl<'init: 'ma, 'ma> Framebuffer<'init, 'ma> {
    pub fn new(
        render_pass: &Arc<RenderPass<'init>>, flags: vk::FramebufferCreateFlags, width: u32, height: u32, layers: u32,
        attachments: &[FramebufferAttachment<'init, 'ma>]
    ) -> Result<Self, VulkanUniversalError> {
        let mut inner_attachments = Vec::with_capacity(attachments.len());
        let mut constructed_attachments = Vec::with_capacity(attachments.len());

        for attachment in attachments {
            let image_view = VulkanImageView::new(attachment.image, &attachment.create_info)?;

            inner_attachments.push(image_view.inner());
            constructed_attachments.push(image_view);
        }

        let vk_create_info = vk::FramebufferCreateInfo {
            s_type: vk::StructureType::FRAMEBUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags,
            render_pass: render_pass.inner(),
            attachment_count: inner_attachments.len() as u32,
            p_attachments: inner_attachments.as_ptr(),
            width,
            height,
            layers
        };

        let initialized = render_pass.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_framebuffer(&vk_create_info, None)
        }?;

        Ok(Self {
            inner,
            extent: vk::Extent2D { width, height },
            render_pass: render_pass.clone(),
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

impl Drop for Framebuffer<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.render_pass.device().initialized().unwrap().vulkan_device().destroy_framebuffer(
                self.inner, None
            );
        }
    }
}
