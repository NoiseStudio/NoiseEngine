use std::{
    rc::Rc,
    sync::{Arc, Mutex},
};

use ash::vk;
use vma::{AllocationCreateInfo, Alloc};

use super::{errors::universal::VulkanUniversalError, instance::VulkanInstance};

pub(crate) struct MemoryAllocator {
    inner: vma::Allocator
}

impl MemoryAllocator {
    pub fn new(
        instance: &Arc<VulkanInstance>,
        device: Rc<ash::Device>,
        physical_device: vk::PhysicalDevice,
    ) -> Result<Self, VulkanUniversalError> {
        let create_info = vma::AllocatorCreateInfo::new(
            instance.inner(), &device, physical_device
        );
        let inner = vma::Allocator::new(create_info)?;

        Ok(MemoryAllocator { inner })
    }

    pub fn create_buffer(
        &self, buffer_info: &vk::BufferCreateInfo, create_info: &AllocationCreateInfo
    ) -> Result<(vk::Buffer, MemoryBlock), VulkanUniversalError> {
        let allocation = unsafe {
            self.inner.create_buffer(buffer_info, create_info)
        }?;
        Ok((allocation.0, MemoryBlock::new(self, allocation.1)))
    }

    pub fn create_image(
        &self, image_info: &vk::ImageCreateInfo, create_info: &AllocationCreateInfo
    ) -> Result<(vk::Image, MemoryBlock), VulkanUniversalError> {
        let allocation = unsafe {
            self.inner.create_image(image_info, create_info)
        }?;
        Ok((allocation.0, MemoryBlock::new(self, allocation.1)))
    }
}

pub(crate) struct MemoryBlock<'ma> {
    allocator: &'ma MemoryAllocator,
    inner: Mutex<vma::Allocation>
}

impl<'ma> MemoryBlock<'ma> {
    fn new(
        allocator: &'ma MemoryAllocator,
        inner: vma::Allocation,
    ) -> Self {
        MemoryBlock {
            allocator,
            inner: Mutex::new(inner)
        }
    }

    pub fn allocator(&self) -> &MemoryAllocator {
        self.allocator
    }

    pub fn read(&self, buffer: &mut [u8], start: u64) -> Result<(), VulkanUniversalError> {
        let allocator = &self.allocator().inner;
        let mut allocation = self.inner.lock().unwrap();
        let ptr = unsafe { allocator.map_memory(&mut allocation) }?;
        // TODO: Support not coherent.

        unsafe {
            std::ptr::copy_nonoverlapping(
                ptr.offset(start as isize), buffer.as_mut_ptr(), buffer.len()
            );
        }

        unsafe {
            allocator.unmap_memory(&mut allocation)
        };
        Ok(())
    }

    pub fn write(&self, data: &[u8], start: u64) -> Result<(), VulkanUniversalError> {
        let allocator = &self.allocator().inner;
        let mut allocation = self.inner.lock().unwrap();
        let ptr = unsafe { allocator.map_memory(&mut allocation) }?;
        // TODO: Support not coherent.

        unsafe {
            std::ptr::copy_nonoverlapping(
                data.as_ptr(), ptr.offset(start as isize), data.len()
            );
        }

        unsafe {
            allocator.unmap_memory(&mut allocation)
        };
        Ok(())
    }
}

impl Drop for MemoryBlock<'_> {
    fn drop(&mut self) {
        unsafe {
            let mut allocation = self.inner.lock().unwrap();
            self.allocator.inner.free_memory(&mut allocation);
        }
    }
}
