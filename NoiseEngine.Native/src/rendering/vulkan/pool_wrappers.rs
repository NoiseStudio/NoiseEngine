use ash::vk;

pub struct VulkanCommandPool<'init> {
    vulkan_device: &'init ash::Device,
    inner: vk::CommandPool
}

impl<'init> VulkanCommandPool<'init> {
    pub(super) fn new(vulkan_device: &'init ash::Device, inner: vk::CommandPool) -> Self {
        Self { vulkan_device, inner }
    }

    pub fn inner(&self) -> vk::CommandPool {
        self.inner
    }
}

impl Drop for VulkanCommandPool<'_> {
    fn drop(&mut self) {
        unsafe {
            self.vulkan_device.destroy_command_pool(self.inner, None);
        }
    }
}
