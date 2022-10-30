use std::ptr;

use ash::vk;

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError, fence::VulkanFence};

pub struct VulkanDevicePool {
    device_ptr: *const VulkanDevice
}

impl VulkanDevicePool {
    pub fn new(device: &VulkanDevice) -> Self {
        Self {
            device_ptr: device
        }
    }

    pub fn device(&self) -> &VulkanDevice {
        unsafe {
            &*self.device_ptr
        }
    }

    pub fn get_fence(&self) -> Result<VulkanFence, VulkanUniversalError> {
        let initialized = self.device().initialized()?;

        let create_info = vk::FenceCreateInfo {
            s_type: vk::StructureType::FENCE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FenceCreateFlags::empty(),
        };

        let fence = unsafe {
            initialized.vulkan_device().create_fence(&create_info, None)
        }?;

        Ok(VulkanFence::new(self, fence))
    }
}
