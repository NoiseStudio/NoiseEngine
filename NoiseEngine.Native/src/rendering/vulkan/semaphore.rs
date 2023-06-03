use std::sync::Arc;

use ash::vk;

use super::device::VulkanDevice;

pub struct VulkanSemaphore<'init> {
    inner: vk::Semaphore,
    device: Arc<VulkanDevice<'init>>,
}

impl<'init> VulkanSemaphore<'init> {
    pub fn new(device: &Arc<VulkanDevice<'init>>, inner: vk::Semaphore) -> Self {
        Self {
            inner,
            device: device.clone(),
        }
    }

    pub fn inner(&self) -> vk::Semaphore {
        self.inner
    }
}

impl Drop for VulkanSemaphore<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_semaphore(self.inner, None);
        }
    }
}
