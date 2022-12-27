use std::{ptr, sync::Arc, collections::{HashMap, hash_map::Entry}};

use ash::vk;

use crate::rendering::vulkan::{
    device::VulkanDevice, errors::universal::VulkanUniversalError
};

use super::pool_sizes::DescriptorPoolSizes;

pub struct DescriptorSetLayout<'init> {
    inner: vk::DescriptorSetLayout,
    pool_sizes: Arc<DescriptorPoolSizes>,
    device: Arc<VulkanDevice<'init>>,
}

impl<'dev: 'init, 'init> DescriptorSetLayout<'init> {
    pub fn new(
        device: &'dev Arc<VulkanDevice<'init>>, flags: vk::DescriptorSetLayoutCreateFlags,
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

        let mut pool_sizes = HashMap::new();
        let mut sum_count = 0;

        for binding in bindings.iter() {
            let count = binding.descriptor_count;
            sum_count += count;

            match pool_sizes.entry(binding.descriptor_type) {
                Entry::Occupied(mut o) => _ = o.insert(o.get() + count),
                Entry::Vacant(v) => _ = v.insert(count),
            };
        }

        Ok(Self {
            inner,
            pool_sizes: Arc::new(DescriptorPoolSizes { map: pool_sizes, count: sum_count }),
            device: device.clone(),
        })
    }

    pub fn inner(&self) -> vk::DescriptorSetLayout {
        self.inner
    }

    pub fn pool_sizes(&self) -> &Arc<DescriptorPoolSizes> {
        &self.pool_sizes
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }
}

impl Drop for DescriptorSetLayout<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device.initialized().unwrap().vulkan_device().destroy_descriptor_set_layout(
                self.inner, None
            );
        }
    }
}
