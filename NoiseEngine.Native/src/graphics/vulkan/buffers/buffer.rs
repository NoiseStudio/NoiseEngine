use std::{sync::Arc, ops::Range};

use enumflags2::BitFlags;
use vulkano::{
    buffer::{sys::{UnsafeBuffer, UnsafeBufferCreateInfo}, BufferUsage, BufferCreationError},
    DeviceSize, device::Device, sync::Sharing,
    memory::{MemoryPool, pool::{
        AllocLayout, MappingRequirement, AllocFromRequirementsFilter, MemoryPoolAlloc, PotentialDedicatedAllocation,
        StandardMemoryPoolAlloc}, DedicatedAllocation, DeviceMemoryError, MappedDeviceMemory, MemoryMapError
    }
};

use crate::{graphics::{
    vulkan::{device::VulkanDevice, errors::buffer_create::VulkanBufferCreateError},
    buffers::{buffer_usage::GraphicsBufferUsage, buffer::GraphicsBuffer}
}, interop::prelude::InteropResult, errors::invalid_operation::InvalidOperationError};

pub(crate) struct VulkanBuffer<'a, A = PotentialDedicatedAllocation<StandardMemoryPoolAlloc>> {
    // TODO: use device and buffer.
    _device: &'a VulkanDevice,
    _buffer: Arc<UnsafeBuffer>,
    memory: A
}

impl<'a> VulkanBuffer<'a> {
    pub fn new(
        device: &'a VulkanDevice, usage: BitFlags<GraphicsBufferUsage>, size: DeviceSize, map: bool
    ) -> Result<Self, VulkanBufferCreateError> {
        let vulkan_device = device.device()?;
        let buffer = Self::create_buffer(vulkan_device, size, usage)?;

        let memory = MemoryPool::alloc_from_requirements(
            &vulkan_device.standard_memory_pool(),
            &buffer.memory_requirements(),
            AllocLayout::Linear,
            match map {
                true => MappingRequirement::Map,
                false => MappingRequirement::DoNotMap
            }, 
            Some(DedicatedAllocation::Buffer(&buffer)),
            |m| {
                if map {
                    if m.property_flags.host_cached {
                        AllocFromRequirementsFilter::Allowed
                    } else {
                        AllocFromRequirementsFilter::Preferred
                    }
                } else {
                    if m.property_flags.device_local {
                        AllocFromRequirementsFilter::Preferred
                    } else {
                        AllocFromRequirementsFilter::Allowed
                    }
                }
            }
        )?;

        match unsafe {
            buffer.bind_memory(memory.memory(), memory.offset())
        } {
            Ok(_) => (),
            Err(oom) => return Err(DeviceMemoryError::from(oom).into()),
        }

        Ok(VulkanBuffer { _device: device, _buffer: buffer, memory })
    }

    fn create_buffer(
        vulkan_device: &Arc<Device>, size: DeviceSize, usage: BitFlags<GraphicsBufferUsage>
    ) -> Result<Arc<UnsafeBuffer>, BufferCreationError> {
        let vulkan_usage = BufferUsage {
            transfer_src: usage.contains(GraphicsBufferUsage::TransferSource),
            transfer_dst: usage.contains(GraphicsBufferUsage::TransferDestination),
            uniform_texel_buffer: usage.contains(GraphicsBufferUsage::UniformTexel),
            storage_texel_buffer: usage.contains(GraphicsBufferUsage::StorageTexel),
            uniform_buffer: usage.contains(GraphicsBufferUsage::Uniform),
            storage_buffer: usage.contains(GraphicsBufferUsage::Storage),
            index_buffer: usage.contains(GraphicsBufferUsage::Index),
            vertex_buffer: usage.contains(GraphicsBufferUsage::Vertex),
            indirect_buffer: usage.contains(GraphicsBufferUsage::Indirect),
            ..Default::default()
        };

        let create_info = UnsafeBufferCreateInfo {
            sharing: if vulkan_device.active_queue_family_indices().len() >= 2 {
                Sharing::Concurrent(vulkan_device.active_queue_family_indices().into_iter().map(|&x| x).collect())
            } else {
                Sharing::Exclusive
            },
            size,
            usage: vulkan_usage,
            ..Default::default()
        };

        UnsafeBuffer::new(vulkan_device.clone(), create_info)
    }

    fn get_mapped_memory(&self) -> Result<&MappedDeviceMemory, InvalidOperationError> {
        match self.memory.mapped_memory() {
            Some(m) => Ok(m),
            None => Err(InvalidOperationError::with_str("This VulkanBuffer does not mapped memory."))
        }
    }

    fn mapped_memory_invalidate_range(
        &self, mapped_memory: &MappedDeviceMemory, start: u64, length: u64
    ) -> Result<Range<u64>, MemoryMapError> {
        let offset = self.memory.offset() + start;
        let memory_range = offset..offset + length;

        unsafe {
            mapped_memory.invalidate_range(memory_range.clone())?;
        }

        Ok(memory_range)
    }
}

impl<'a> GraphicsBuffer for VulkanBuffer<'a> {
    fn host_read(&self, buffer: &mut [u8], start: u64) -> InteropResult<()> {
        let mapped_memory = match self.get_mapped_memory() {
            Ok(m) => m,
            Err(err) => return InteropResult::with_err(err.into())
        };

        let memory_range = {
            match self.mapped_memory_invalidate_range(mapped_memory, start, buffer.len() as u64) {
                Ok(r) => r,
                Err(err) => return InteropResult::with_err(err.into())
            }
        };

        let bytes = match unsafe { mapped_memory.read(memory_range) } {
            Ok(bytes) => bytes,
            Err(err) => return InteropResult::with_err(err.into())
        };

        buffer.copy_from_slice(bytes);

        InteropResult::with_ok(())
    }

    fn host_write(&self, data: &[u8], start: u64) -> InteropResult<()> {
        let mapped_memory = match self.get_mapped_memory() {
            Ok(m) => m,
            Err(err) => return InteropResult::with_err(err.into())
        };

        let memory_range = {
            match self.mapped_memory_invalidate_range(mapped_memory, start, data.len() as u64) {
                Ok(r) => r,
                Err(err) => return InteropResult::with_err(err.into())
            }
        };

        let bytes = match unsafe { mapped_memory.write(memory_range) } {
            Ok(bytes) => bytes,
            Err(err) => return InteropResult::with_err(err.into())
        };

        bytes.copy_from_slice(data);

        InteropResult::with_ok(())
    }
}
