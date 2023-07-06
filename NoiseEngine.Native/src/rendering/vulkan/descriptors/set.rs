use std::{mem::ManuallyDrop, ptr, sync::Arc};

use ash::vk;
use libc::c_void;

use crate::{
    common::pool::PoolItem,
    rendering::vulkan::{
        errors::universal::VulkanUniversalError, pool_wrappers::VulkanDescriptorPool,
    },
};

use super::{set_layout::DescriptorSetLayout, update_template::DescriptorUpdateTemplate};

pub struct DescriptorSet<'init> {
    layout: Arc<DescriptorSetLayout<'init>>,
    inner: vk::DescriptorSet,
    pool: ManuallyDrop<PoolItem<'init, VulkanDescriptorPool<'init>>>,
}

impl<'init> DescriptorSet<'init> {
    pub fn new(
        layout: &'init Arc<DescriptorSetLayout<'init>>,
    ) -> Result<Self, VulkanUniversalError> {
        let initialized = layout.device().initialized()?;

        let pool = initialized
            .pool()
            .get_descriptor_pool(layout.pool_sizes())?;

        let allocate_info = vk::DescriptorSetAllocateInfo {
            s_type: vk::StructureType::DESCRIPTOR_SET_ALLOCATE_INFO,
            p_next: ptr::null(),
            descriptor_pool: pool.inner(),
            descriptor_set_count: 1,
            p_set_layouts: &layout.inner(),
        };

        let inner = unsafe {
            initialized
                .vulkan_device()
                .allocate_descriptor_sets(&allocate_info)
        }?[0];

        Ok(Self {
            layout: layout.clone(),
            inner,
            pool: ManuallyDrop::new(pool),
        })
    }

    pub fn inner(&self) -> vk::DescriptorSet {
        self.inner
    }

    pub fn update(&self, template: &DescriptorUpdateTemplate, data: &[u8]) {
        unsafe {
            self.layout
                .device()
                .initialized()
                .unwrap()
                .vulkan_device()
                .update_descriptor_set_with_template(
                    self.inner,
                    template.inner(),
                    data.as_ptr() as *const c_void,
                );
        }
    }
}

impl Drop for DescriptorSet<'_> {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.pool);
        }

        unsafe {
            self.layout
                .device()
                .initialized()
                .unwrap()
                .vulkan_device()
                .reset_descriptor_pool(self.pool.inner(), vk::DescriptorPoolResetFlags::default())
        }
        .unwrap();
    }
}
