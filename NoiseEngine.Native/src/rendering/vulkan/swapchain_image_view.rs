use std::{ptr, sync::Arc};

use ash::vk;

use super::{swapchain::Swapchain, errors::universal::VulkanUniversalError, device::VulkanDevice};

pub struct SwapchainImageView<'init> {
    inner: vk::ImageView,
    device: Arc<VulkanDevice<'init>>
}

impl<'init> SwapchainImageView<'init> {
    pub fn new(swapchain: &Swapchain<'init, '_>, image: vk::Image) -> Result<Self, VulkanUniversalError> {
        let vk_create_info = vk::ImageViewCreateInfo {
            s_type: vk::StructureType::IMAGE_VIEW_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::ImageViewCreateFlags::empty(),
            image,
            view_type: vk::ImageViewType::TYPE_2D,
            format: swapchain.format().format,
            components: vk::ComponentMapping {
                r: vk::ComponentSwizzle::IDENTITY,
                g: vk::ComponentSwizzle::IDENTITY,
                b: vk::ComponentSwizzle::IDENTITY,
                a: vk::ComponentSwizzle::IDENTITY,
            },
            subresource_range: vk::ImageSubresourceRange {
                aspect_mask: vk::ImageAspectFlags::COLOR,
                base_mip_level: 0,
                level_count: 1,
                base_array_layer: 0,
                layer_count: 1,
            },
        };

        let initialized = swapchain.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_image_view(&vk_create_info, None)
        }?;

        Ok(Self { inner, device: swapchain.device().clone() })
    }

    pub fn inner(&self) -> vk::ImageView {
        self.inner
    }
}

impl Drop for SwapchainImageView<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device.initialized().unwrap().vulkan_device().destroy_image_view(
                self.inner, None
            );
        }
    }
}
