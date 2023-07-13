use std::{ptr, sync::Arc};

use ash::vk;

use crate::{
    interop::prelude::InteropResult,
    rendering::{
        buffers::buffer::GraphicsBuffer,
        vulkan::{
            device::{VulkanDevice, VulkanDeviceInitialized},
            errors::universal::VulkanUniversalError,
            memory_allocator::MemoryBlock,
        },
    },
};

pub(crate) struct VulkanBuffer<'init: 'ma, 'ma> {
    buffer: vk::Buffer,
    memory: MemoryBlock<'ma>,
    device: Arc<VulkanDevice<'init>>,
}

impl<'dev: 'init, 'init: 'ma, 'ma> VulkanBuffer<'init, 'ma> {
    pub fn new(
        device: &'dev Arc<VulkanDevice<'init>>,
        usage: vk::BufferUsageFlags,
        size: u64,
        map: bool,
    ) -> Result<Self, VulkanUniversalError> {
        let initialized = device.initialized()?;

        let buffer_info = Self::create_buffer_info(initialized, size, usage);
        let alloc_info = vma::AllocationCreateInfo {
            flags: match map {
                true => vma::AllocationCreateFlags::HOST_ACCESS_SEQUENTIAL_WRITE,
                false => vma::AllocationCreateFlags::empty(),
            },
            usage: match map {
                true => vma::MemoryUsage::AutoPreferHost,
                false => vma::MemoryUsage::AutoPreferDevice,
            },
            required_flags: match map {
                true => vk::MemoryPropertyFlags::HOST_VISIBLE | vk::MemoryPropertyFlags::HOST_COHERENT,
                false => vk::MemoryPropertyFlags::DEVICE_LOCAL,
            },
            ..Default::default()
        };
        let (buffer, memory) = initialized.allocator().create_buffer(
            &buffer_info, &alloc_info
        )?;

        Ok(VulkanBuffer {
            buffer,
            memory,
            device: device.clone(),
        })
    }

    pub fn inner(&self) -> vk::Buffer {
        self.buffer
    }

    fn create_buffer_info(
        initialized: &VulkanDeviceInitialized,
        size: u64,
        usage: vk::BufferUsageFlags,
    ) -> vk::BufferCreateInfo {
        let sharing_mode;
        let queue_family_index_count;
        let p_queue_family_indices;
        let mut p_queue_family_indices_vec;

        let count = initialized.queue_families_count() as u32;
        if count >= 2 {
            p_queue_family_indices_vec = Vec::with_capacity(count as usize);
            for i in 0..count {
                p_queue_family_indices_vec.push(i);
            }

            sharing_mode = vk::SharingMode::CONCURRENT;
            queue_family_index_count = count;
            p_queue_family_indices = p_queue_family_indices_vec.as_ptr();
        } else {
            sharing_mode = vk::SharingMode::EXCLUSIVE;
            queue_family_index_count = 0;
            p_queue_family_indices = ptr::null();
        }

        vk::BufferCreateInfo {
            s_type: vk::StructureType::BUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::BufferCreateFlags::empty(),
            size,
            usage,
            sharing_mode,
            queue_family_index_count,
            p_queue_family_indices,
        }
    }
}

impl Drop for VulkanBuffer<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_buffer(self.buffer, None);
        }
    }
}

impl GraphicsBuffer for VulkanBuffer<'_, '_> {
    fn host_read(&self, buffer: &mut [u8], start: u64) -> InteropResult<()> {
        match self.memory.read(buffer, start) {
            Ok(()) => InteropResult::with_ok(()),
            Err(err) => InteropResult::with_err(err.into()),
        }
    }

    fn host_write(&self, data: &[u8], start: u64) -> InteropResult<()> {
        match self.memory.write(data, start) {
            Ok(()) => InteropResult::with_ok(()),
            Err(err) => InteropResult::with_err(err.into()),
        }
    }
}
