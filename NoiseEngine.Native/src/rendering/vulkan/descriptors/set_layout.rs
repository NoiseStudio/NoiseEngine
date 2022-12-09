use std::ptr;

use ash::vk;

use crate::rendering::vulkan::{
    device::{VulkanDeviceInitialized, VulkanDevice}, errors::universal::VulkanUniversalError
};

pub struct DescriptorSetLayout<'init> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::DescriptorSetLayout
}

impl<'dev: 'init, 'init> DescriptorSetLayout<'init> {
    pub fn new(
        device: &'dev VulkanDevice<'_, 'init>, flags: vk::DescriptorSetLayoutCreateFlags,
        bindings: &[vk::DescriptorSetLayoutBinding]
    ) -> Result<Self, VulkanUniversalError> {
        let create_info = vk::DescriptorSetLayoutCreateInfo {
            s_type: vk::StructureType::DESCRIPTOR_SET_LAYOUT_CREATE_INFO,
            p_next: ptr::null(),
            flags,
            binding_count: bindings.len() as u32,
            p_bindings: bindings.as_ptr(),
        };

        let initialized = device.initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_descriptor_set_layout(&create_info, None)
        }?;

        Ok(Self { initialized, inner })
    }

    pub fn inner(&self) -> vk::DescriptorSetLayout {
        self.inner
    }

    pub(crate) fn initialized(&self) ->  &'init VulkanDeviceInitialized<'init> {
        self.initialized
    }
}

impl Drop for DescriptorSetLayout<'_> {
    fn drop(&mut self) {
        unsafe {
            self.initialized.vulkan_device().destroy_descriptor_set_layout(
                self.inner, None
            );
        }
    }
}
