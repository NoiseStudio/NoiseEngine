use std::{ptr, rc::Rc};

use ash::vk;

use super::{errors::universal::VulkanUniversalError, fence::VulkanFence};

pub struct VulkanDevicePool {
    vulkan_device: Rc<ash::Device>,
}

impl VulkanDevicePool {
    pub(super) fn new(device: Rc<ash::Device>) -> Self {
        Self {
            vulkan_device: device
        }
    }

    pub fn vulkan_device(&self) -> &ash::Device {
        &self.vulkan_device
    }

    pub fn get_fence(&self) -> Result<VulkanFence, VulkanUniversalError> {
        let create_info = vk::FenceCreateInfo {
            s_type: vk::StructureType::FENCE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FenceCreateFlags::empty(),
        };

        let fence = unsafe {
            self.vulkan_device.create_fence(&create_info, None)
        }?;

        Ok(VulkanFence::new(self, fence))
    }
}
