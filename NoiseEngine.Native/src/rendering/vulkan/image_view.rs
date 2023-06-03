use std::{ptr, sync::Arc};

use ash::vk;

use super::{errors::universal::VulkanUniversalError, image::VulkanImage};

#[repr(C)]
pub struct VulkanImageViewCreateInfo {
    pub flags: vk::ImageViewCreateFlags,
    pub view_type: vk::ImageViewType,
    pub components: vk::ComponentMapping,
    pub subresource_range: vk::ImageSubresourceRange,
}

pub struct VulkanImageView<'init: 'ma, 'ma> {
    inner: vk::ImageView,
    image: Arc<VulkanImage<'init, 'ma>>,
}

impl<'init: 'ma, 'ma> VulkanImageView<'init, 'ma> {
    pub fn new(
        image: &Arc<VulkanImage<'init, 'ma>>,
        create_info: &VulkanImageViewCreateInfo,
    ) -> Result<Self, VulkanUniversalError> {
        let vk_create_info = vk::ImageViewCreateInfo {
            s_type: vk::StructureType::IMAGE_VIEW_CREATE_INFO,
            p_next: ptr::null(),
            flags: create_info.flags,
            image: image.inner(),
            view_type: create_info.view_type,
            format: image.format(),
            components: create_info.components,
            subresource_range: create_info.subresource_range,
        };

        let initialized = image.device().initialized()?;
        let inner = unsafe {
            initialized
                .vulkan_device()
                .create_image_view(&vk_create_info, None)
        }?;

        Ok(Self {
            inner,
            image: image.clone(),
        })
    }

    pub fn inner(&self) -> vk::ImageView {
        self.inner
    }
}

impl Drop for VulkanImageView<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.image
                .device()
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_image_view(self.inner, None);
        }
    }
}
