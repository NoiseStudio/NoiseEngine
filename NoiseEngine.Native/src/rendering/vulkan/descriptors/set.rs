use std::ptr;

use ash::vk;
use libc::c_void;

use crate::{
    common::pool::PoolItem,
    rendering::vulkan::{
        device::VulkanDeviceInitialized, pool_wrappers::VulkanDescriptorPool, errors::universal::VulkanUniversalError
    }
};

use super::{update_template::DescriptorUpdateTemplate, set_layout::DescriptorSetLayout};

pub struct DescriptorSet<'init> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::DescriptorSet,
    pool: PoolItem<'init, VulkanDescriptorPool<'init>>
}

impl<'init: 'setl, 'setl> DescriptorSet<'init> {
    pub fn new(layout: &'setl DescriptorSetLayout<'init>) -> Result<Self, VulkanUniversalError> {
        let initialized = layout.initialized();

        let pool = initialized.pool().get_descriptor_pool(
            layout.pool_sizes()
        )?;

        let allocate_info = vk::DescriptorSetAllocateInfo {
            s_type: vk::StructureType::DESCRIPTOR_SET_ALLOCATE_INFO,
            p_next: ptr::null(),
            descriptor_pool: pool.inner(),
            descriptor_set_count: 1,
            p_set_layouts: &layout.inner(),
        };

        let inner = unsafe {
            initialized.vulkan_device().allocate_descriptor_sets(&allocate_info)
        }?[0];

        Ok(Self { initialized, inner, pool })
    }

    pub fn inner(&self) -> vk::DescriptorSet {
        self.inner
    }

    pub fn update(&self, template: &DescriptorUpdateTemplate, data: &[u8]) {
        unsafe {
            self.initialized.vulkan_device().update_descriptor_set_with_template(
                self.inner, template.inner(),
                data.as_ptr() as *const c_void
            );
        }
    }
}

impl Drop for DescriptorSet<'_> {
    fn drop(&mut self) {
        unsafe {
            self.initialized.vulkan_device().reset_descriptor_pool(
                self.pool.inner(), vk::DescriptorPoolResetFlags::default()
            )
        }.unwrap();
    }
}
