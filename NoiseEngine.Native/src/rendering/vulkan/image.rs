use std::{ptr, sync::Arc};

use ash::vk;

use crate::rendering::texture::Texture;

use super::{
    device::VulkanDevice, errors::universal::VulkanUniversalError, memory_allocator::MemoryBlock,
};

#[repr(C)]
pub struct VulkanImageCreateInfo {
    pub flags: vk::ImageCreateFlags,
    pub image_type: vk::ImageType,
    pub extent: vk::Extent3D,
    pub format: vk::Format,
    pub mip_levels: u32,
    pub array_layers: u32,
    pub sample_count: u32,
    pub linear: bool,
    pub usage: vk::ImageUsageFlags,
    pub concurrent: bool,
    pub layout: vk::ImageLayout,
}

pub struct VulkanImage<'init: 'ma, 'ma> {
    inner: vk::Image,
    format: vk::Format,
    layout: vk::ImageLayout,
    _memory: MemoryBlock<'ma>,
    device: Arc<VulkanDevice<'init>>,
}

impl<'init: 'ma, 'ma> VulkanImage<'init, 'ma> {
    pub fn new(
        device: &'ma Arc<VulkanDevice<'init>>,
        create_info: VulkanImageCreateInfo,
    ) -> Result<Self, VulkanUniversalError> {
        let initialized = device.initialized()?;

        let mut queue_family_indices;
        let sharing_mode;

        if create_info.concurrent {
            let queue_families = initialized.get_families();

            if queue_families.len() <= 1 {
                queue_family_indices = Vec::new();
                sharing_mode = vk::SharingMode::EXCLUSIVE;
            } else {
                queue_family_indices = Vec::with_capacity(queue_families.len());
                sharing_mode = vk::SharingMode::CONCURRENT;

                for family in queue_families {
                    queue_family_indices.push(family.index());
                }
            }
        } else {
            queue_family_indices = Vec::new();
            sharing_mode = vk::SharingMode::EXCLUSIVE;
        }

        let vk_create_info = vk::ImageCreateInfo {
            s_type: vk::StructureType::IMAGE_CREATE_INFO,
            p_next: ptr::null(),
            flags: create_info.flags,
            image_type: create_info.image_type,
            format: create_info.format,
            extent: create_info.extent,
            mip_levels: create_info.mip_levels,
            array_layers: create_info.array_layers,
            samples: vk::SampleCountFlags::from_raw(create_info.sample_count),
            tiling: match create_info.linear {
                false => vk::ImageTiling::OPTIMAL,
                true => vk::ImageTiling::LINEAR,
            },
            usage: create_info.usage,
            sharing_mode,
            queue_family_index_count: queue_family_indices.len() as u32,
            p_queue_family_indices: queue_family_indices.as_ptr(),
            initial_layout: vk::ImageLayout::UNDEFINED,
        };

        let inner = unsafe {
            initialized
                .vulkan_device()
                .create_image(&vk_create_info, None)
        }?;

        // Memory.
        let memory_requirements = unsafe {
            initialized
                .vulkan_device()
                .get_image_memory_requirements(inner)
        };

        let memory = initialized.allocator().alloc_memory_type(
            memory_requirements.size,
            memory_requirements.memory_type_bits,
            vk::MemoryPropertyFlags::DEVICE_LOCAL,
        )?;

        unsafe {
            initialized
                .vulkan_device()
                .bind_image_memory(inner, memory.memory(), memory.offset())
        }?;

        Ok(Self {
            inner,
            format: create_info.format,
            layout: create_info.layout,
            _memory: memory,
            device: device.clone(),
        })
    }

    pub fn format(&self) -> vk::Format {
        self.format
    }

    pub fn layout(&self) -> vk::ImageLayout {
        self.layout
    }

    pub fn inner(&self) -> vk::Image {
        self.inner
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }
}

impl Drop for VulkanImage<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_image(self.inner, None)
        }
    }
}

impl Texture for VulkanImage<'_, '_> {}
