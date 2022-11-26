use ash::vk;

use super::device_pool::VulkanDevicePool;

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

pub struct VulkanDescriptorPool<'devpool> {
    pool: &'devpool VulkanDevicePool<'devpool>,
    inner: vk::DescriptorPool
}

impl<'devpool> VulkanDescriptorPool<'devpool> {
    pub(super) fn new(pool: &'devpool VulkanDevicePool<'devpool>, inner: vk::DescriptorPool) -> Self {
        Self { pool, inner }
    }

    pub fn inner(&self) -> vk::DescriptorPool {
        self.inner
    }
}

impl Drop for VulkanDescriptorPool<'_> {
    fn drop(&mut self) {
        unsafe {
            self.pool.vulkan_device().destroy_descriptor_pool(self.inner, None);
        }
    }
}
