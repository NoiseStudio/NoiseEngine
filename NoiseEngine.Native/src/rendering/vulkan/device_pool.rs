use std::{ptr, rc::Rc, sync::Arc};

use ash::vk;

use crate::common::pool::{Pool, PoolItem};

use super::{
    errors::universal::VulkanUniversalError, fence::VulkanFence, pool_wrappers::VulkanDescriptorPool,
    descriptors::pool_sizes::DescriptorPoolSizes, device::VulkanDevice, semaphore::VulkanSemaphore
};

pub struct VulkanDevicePool<'devpool> {
    vulkan_device: Rc<ash::Device>,
    descriptor_pools: Pool<VulkanDescriptorPool<'devpool>>
}

impl<'init: 'devpool, 'devpool> VulkanDevicePool<'devpool> {
    pub(super) fn new(device: Rc<ash::Device>) -> Self {
        Self {
            vulkan_device: device,
            descriptor_pools: Pool::default()
        }
    }

    pub fn vulkan_device(&self) -> &ash::Device {
        &self.vulkan_device
    }

    pub fn get_fence(
        &self, device: &Arc<VulkanDevice<'init>>
    ) -> Result<VulkanFence<'init>, VulkanUniversalError> {
        let create_info = vk::FenceCreateInfo {
            s_type: vk::StructureType::FENCE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::FenceCreateFlags::empty(),
        };

        let fence = unsafe {
            self.vulkan_device.create_fence(&create_info, None)
        }?;

        Ok(VulkanFence::new(device, fence))
    }

    pub fn get_semaphore(
        &self, device: &Arc<VulkanDevice<'init>>
    ) -> Result<VulkanSemaphore<'init>, VulkanUniversalError> {
        let create_info = vk::SemaphoreCreateInfo {
            s_type: vk::StructureType::SEMAPHORE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::SemaphoreCreateFlags::empty(),
        };

        let semaphore = unsafe {
            self.vulkan_device.create_semaphore(&create_info, None)
        }?;

        Ok(VulkanSemaphore::new(device, semaphore))
    }

    pub fn get_descriptor_pool(
        &'devpool self, pool_sizes: &Arc<DescriptorPoolSizes>
    ) -> Result<PoolItem<'devpool, VulkanDescriptorPool<'devpool>>, VulkanUniversalError> {
        self.descriptor_pools.get_or_create_where(
            |obj| {
                let obj_pool_sizes = obj.pool_sizes();

                if pool_sizes.count > obj_pool_sizes.count {
                    return false
                }

                for (ty, count) in pool_sizes.map.iter() {
                    if let Some(c) = obj_pool_sizes.map.get(ty) {
                        if c >= count {
                            continue
                        }
                    };

                    return false
                }

                true
            },
            || {
                let mut final_pool_sizes = Vec::with_capacity(pool_sizes.map.len());
                for (ty, count) in pool_sizes.map.iter() {
                    final_pool_sizes.push(vk::DescriptorPoolSize {
                        ty: *ty,
                        descriptor_count: *count,
                    });
                }

                let pool_info = vk::DescriptorPoolCreateInfo {
                    s_type: vk::StructureType::DESCRIPTOR_POOL_CREATE_INFO,
                    p_next: ptr::null(),
                    flags: vk::DescriptorPoolCreateFlags::default(),
                    max_sets: 1,
                    pool_size_count: final_pool_sizes.len() as u32,
                    p_pool_sizes: final_pool_sizes.as_ptr(),
                };

                let pool = unsafe {
                    self.vulkan_device.create_descriptor_pool(&pool_info, None)
                }?;

                Ok(VulkanDescriptorPool::new(self, pool, pool_sizes.clone()))
            }
        )
    }
}
