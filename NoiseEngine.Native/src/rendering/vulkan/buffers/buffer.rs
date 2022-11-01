use std::ptr;

use ash::vk;

use crate::{rendering::{
    vulkan::{
        device::VulkanDeviceInitialized,
        errors::universal::VulkanUniversalError,
        memory_allocator::MemoryBlock
    },
    buffers::buffer::GraphicsBuffer
}, interop::prelude::InteropResult};

pub(crate) struct VulkanBuffer<'init: 'ma, 'ma> {
    vulkan_device: &'init ash::Device,
    buffer: vk::Buffer,
    memory: MemoryBlock<'ma>,
}

impl<'init: 'ma, 'ma> VulkanBuffer<'init, 'ma> {
    pub fn new(
        initialized: &'init VulkanDeviceInitialized<'init>, usage: vk::BufferUsageFlags, size: u64, map: bool
    ) -> Result<Self, VulkanUniversalError> {
        let vulkan_device = initialized.vulkan_device();

        let buffer = Self::create_buffer(initialized, size, usage)?;
        let memory = initialized.allocator().alloc(size, map)?;

        unsafe {
            vulkan_device.bind_buffer_memory(buffer, memory.memory(), memory.offset())
        }?;

        Ok(VulkanBuffer { vulkan_device, buffer, memory })
    }

    pub fn inner(&self) -> vk::Buffer {
        self.buffer
    }

    fn create_buffer(
        initialized: &VulkanDeviceInitialized, size: u64, usage: vk::BufferUsageFlags
    ) -> Result<vk::Buffer, VulkanUniversalError> {
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

        let create_info = vk::BufferCreateInfo {
            s_type: vk::StructureType::BUFFER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::BufferCreateFlags::empty(),
            size,
            usage,
            sharing_mode,
            queue_family_index_count,
            p_queue_family_indices
        };

        match unsafe { initialized.vulkan_device().create_buffer(&create_info, None) } {
            Ok(buffer) => Ok(buffer),
            Err(err) => Err(err.into())
        }
    }
}

impl Drop for VulkanBuffer<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.vulkan_device.destroy_buffer(self.buffer, None);
        }
    }
}

impl GraphicsBuffer for VulkanBuffer<'_, '_> {
    fn host_read(&self, buffer: &mut [u8], start: u64) -> InteropResult<()> {
        match self.memory.read(buffer, start) {
            Ok(()) => InteropResult::with_ok(()),
            Err(err) => InteropResult::with_err(err.into())
        }
    }

    fn host_write(&self, data: &[u8], start: u64) -> InteropResult<()> {
        match self.memory.write(data, start) {
            Ok(()) => InteropResult::with_ok(()),
            Err(err) => InteropResult::with_err(err.into())
        }
    }
}
