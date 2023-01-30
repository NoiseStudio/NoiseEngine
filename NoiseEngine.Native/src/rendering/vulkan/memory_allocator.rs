use std::{sync::{Mutex, Arc}, ptr, mem::ManuallyDrop, cell::UnsafeCell, rc::Rc};

use ash::vk;
use gpu_alloc::{Config, Request, UsageFlags};
use gpu_alloc_ash::AshMemoryDevice;

use super::{errors::universal::VulkanUniversalError, instance::VulkanInstance};

pub(crate) struct MemoryAllocator {
    device: Rc<ash::Device>,
    inner: Mutex<gpu_alloc::GpuAllocator<vk::DeviceMemory>>
}

impl MemoryAllocator {
    pub fn new(
        instance: &Arc<VulkanInstance>,
        device: Rc<ash::Device>,
        physical_device: vk::PhysicalDevice,
    ) -> Result<Self, VulkanUniversalError> {
        let props = unsafe {
            gpu_alloc_ash::device_properties(&instance.inner(), 0, physical_device)?
        };

        // TODO: implement best suitable config.
        let config = Config::i_am_prototyping();

        Ok(MemoryAllocator {
            device: device.clone(),
            inner: Mutex::new(gpu_alloc::GpuAllocator::new(config, props))
        })
    }

    pub fn alloc(&self, size: u64, map: bool) -> Result<MemoryBlock, VulkanUniversalError> {
        match unsafe {
            self.inner.lock().unwrap().alloc(self.memory_device(), Request {
                size,
                align_mask: 1,
                usage: if map {
                    UsageFlags::HOST_ACCESS
                } else {
                    UsageFlags::FAST_DEVICE_ACCESS
                },
                memory_types: !0
            })
        } {
            Ok(block) => Ok(MemoryBlock::new(self, block)),
            Err(err) => Err(err.into())
        }
    }

    pub fn alloc_memory_type(
        &self, size: u64, _type_bits: u32, property_flags: vk::MemoryPropertyFlags
    ) -> Result<MemoryBlock, VulkanUniversalError> {
        let mut usage = UsageFlags::empty();
        if property_flags.contains(vk::MemoryPropertyFlags::DEVICE_LOCAL) {
            usage |= UsageFlags::FAST_DEVICE_ACCESS;
        }
        if property_flags.contains(vk::MemoryPropertyFlags::HOST_VISIBLE) {
            usage |= UsageFlags::HOST_ACCESS;
        }
        if property_flags.contains(vk::MemoryPropertyFlags::HOST_COHERENT) {
            usage |= UsageFlags::HOST_ACCESS;
        }
        if property_flags.contains(vk::MemoryPropertyFlags::HOST_CACHED) {
            usage |= UsageFlags::HOST_ACCESS;
        }
        if property_flags.contains(vk::MemoryPropertyFlags::LAZILY_ALLOCATED) {
            usage |= UsageFlags::DEVICE_ADDRESS;
        }

        match unsafe {
            self.inner.lock().unwrap().alloc(self.memory_device(), Request {
                size,
                align_mask: 1,
                usage,
                memory_types: !0
            })
        } {
            Ok(block) => Ok(MemoryBlock::new(self, block)),
            Err(err) => Err(err.into())
        }
    }

    fn memory_device(&self) -> &AshMemoryDevice {
        AshMemoryDevice::wrap(&self.device)
    }
}

impl Drop for MemoryAllocator {
    fn drop(&mut self) {
        unsafe {
            self.inner.lock().unwrap().cleanup(self.memory_device());
        }
    }
}

pub(crate) struct MemoryBlock<'ma> {
    allocator: &'ma MemoryAllocator,
    inner: UnsafeCell<ManuallyDrop<gpu_alloc::MemoryBlock<vk::DeviceMemory>>>,
    mutex: Mutex<()>
}

impl<'ma> MemoryBlock<'ma> {
    fn new(allocator: &'ma MemoryAllocator, inner: gpu_alloc::MemoryBlock<vk::DeviceMemory>) -> Self {
        MemoryBlock {
            allocator,
            inner: ManuallyDrop::new(inner).into(),
            mutex: Mutex::new(())
        }
    }

    pub fn allocator(&self) -> &MemoryAllocator {
        self.allocator
    }

    pub fn memory(&self) -> vk::DeviceMemory {
        *unsafe { &*self.inner.get() }.memory()
    }

    pub fn offset(&self) -> u64 {
        unsafe { &*self.inner.get() }.offset()
    }

    pub fn read(&self, buffer: &mut [u8], start: u64) -> Result<(), VulkanUniversalError> {
        let result = {
            let _lock = self.mutex.lock().unwrap();
            unsafe {
                (*self.inner.get()).read_bytes(self.allocator().memory_device(), start, buffer)
            }
        };

        match result {
            Ok(()) => Ok(()),
            Err(err) => Err(err.into())
        }
    }

    pub fn write(&self, data: &[u8], start: u64) -> Result<(), VulkanUniversalError> {
        let result = {
            let _lock = self.mutex.lock().unwrap();
            unsafe {
                (*self.inner.get()).write_bytes(self.allocator().memory_device(), start, data)
            }
        };

        match result {
            Ok(()) => Ok(()),
            Err(err) => Err(err.into())
        }
    }
}

impl Drop for MemoryBlock<'_> {
    fn drop(&mut self) {
        let block = unsafe {
            ptr::read(self.inner.get_mut() as &gpu_alloc::MemoryBlock<vk::DeviceMemory>)
        };

        unsafe {
            self.allocator().inner.lock().unwrap().dealloc(self.allocator().memory_device(), block)
        };
    }
}
