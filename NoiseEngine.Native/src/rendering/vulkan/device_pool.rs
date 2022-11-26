use std::{ptr, rc::Rc};

use ash::vk;

use crate::common::pool::{Pool, PoolItem};

use super::{errors::universal::VulkanUniversalError, fence::VulkanFence, pool_wrappers::VulkanDescriptorPool};

pub struct VulkanDevicePool<'devpool> {
    vulkan_device: Rc<ash::Device>,
    descriptor_pools: Pool<VulkanDescriptorPool<'devpool>>
}

impl<'devpool> VulkanDevicePool<'devpool> {
    pub(super) fn new(device: Rc<ash::Device>) -> Self {
        Self {
            vulkan_device: device,
            descriptor_pools: Pool::default()
        }
    }

    pub fn vulkan_device(&self) -> &ash::Device {
        &self.vulkan_device
    }

    pub fn get_fence(&'devpool self) -> Result<VulkanFence, VulkanUniversalError> {
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

    pub fn get_descriptor_pool(
        &'devpool self
    ) -> Result<PoolItem<'devpool, VulkanDescriptorPool<'devpool>>, VulkanUniversalError> {
        self.descriptor_pools.get_or_create(|| {
            let pool_size = vk::DescriptorPoolSize {
                ty: vk::DescriptorType::UNIFORM_BUFFER,
                descriptor_count: 1,
            };

            let pool_info = vk::DescriptorPoolCreateInfo {
                s_type: vk::StructureType::DESCRIPTOR_POOL_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::DescriptorPoolCreateFlags::default(),
                max_sets: 1,
                pool_size_count: 1,
                p_pool_sizes: &pool_size,
            };

            let pool = unsafe {
                self.vulkan_device.create_descriptor_pool(&pool_info, None)
            }?;

            Ok(VulkanDescriptorPool::new(self, pool))
        })
    }
}
