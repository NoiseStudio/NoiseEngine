use std::{sync::Arc, cmp};

use ash::vk;

use super::{errors::universal::VulkanUniversalError, swapchain::SwapchainShared};

pub struct SwapchainSupport<'init: 'fam, 'fam> {
    pub capabilities: vk::SurfaceCapabilitiesKHR,
    shared: Arc<SwapchainShared<'init, 'fam>>
}

impl<'init: 'fam, 'fam> SwapchainSupport<'init, 'fam> {
    pub fn new(shared: &Arc<SwapchainShared<'init, 'fam>>) -> Result<Self, VulkanUniversalError> {
        let capabilities = unsafe {
            shared.surface().ash_surface().get_physical_device_surface_capabilities(
                shared.device().physical_device(), shared.surface().inner()
            )
        }?;

        Ok(Self { shared: shared.clone(), capabilities })
    }

    pub fn get_supported_image_count(&self, target_count: u32) -> u32 {
        let used_count;
        if target_count < self.capabilities.min_image_count {
            used_count = self.capabilities.min_image_count;
        } else if target_count > self.capabilities.max_image_count && self.capabilities.max_image_count != 0 {
            used_count = self.capabilities.max_image_count;
        } else {
            used_count = target_count;
        }

        used_count
    }

    pub fn get_extent(&self) -> vk::Extent2D {
        if self.capabilities.current_extent.width != u32::MAX {
            return self.capabilities.current_extent
        }

        let actual_extent = self.shared.surface().window().get_vulkan_extent();

        vk::Extent2D {
            width: cmp::min(
                cmp::max(actual_extent.width, self.capabilities.min_image_extent.width),
                self.capabilities.max_image_extent.width
            ),
            height: cmp::min(
                cmp::max(actual_extent.height, self.capabilities.min_image_extent.height),
                self.capabilities.max_image_extent.height
            )
        }
    }

    pub fn get_pre_transform(&self) -> vk::SurfaceTransformFlagsKHR {
        self.capabilities.current_transform
    }
}
