use ash::vk;

use super::device::VulkanDeviceInitialized;

pub struct VulkanCommandPool {
    initialized_ptr: *const VulkanDeviceInitialized,
    inner: vk::CommandPool
}

impl VulkanCommandPool {
    pub(crate) fn new(initialized: &VulkanDeviceInitialized, inner: vk::CommandPool) -> Self {
        Self { initialized_ptr: initialized, inner }
    }

    pub(crate) fn initialized(&self) -> &VulkanDeviceInitialized {
        unsafe {
            &*self.initialized_ptr
        }
    }

    pub fn inner(&self) -> vk::CommandPool {
        self.inner
    }
}

impl Drop for VulkanCommandPool {
    fn drop(&mut self) {
        unsafe {
            self.initialized().vulkan_device().destroy_command_pool(self.inner, None);
        }
    }
}
