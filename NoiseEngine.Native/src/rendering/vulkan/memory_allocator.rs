use std::{sync::Mutex, ptr, mem::ManuallyDrop};

use ash::vk;
use gpu_alloc::{Config, Request, UsageFlags};
use gpu_alloc_ash::AshMemoryDevice;

use super::{errors::universal::VulkanUniversalError, device::VulkanDevice};

pub(crate) struct MemoryAllocator {
    device_ptr: *const VulkanDevice,
    inner: Mutex<gpu_alloc::GpuAllocator<vk::DeviceMemory>>
}

impl MemoryAllocator {
    pub fn new(device: &VulkanDevice) -> Result<Self, VulkanUniversalError> {
        let props = unsafe {
            gpu_alloc_ash::device_properties(device.instance(), 0, device.physical_device())?
        };

        let config = Config::i_am_potato();

        Ok(MemoryAllocator {
            device_ptr: device,
            inner: Mutex::new(gpu_alloc::GpuAllocator::new(config, props))
        })
    }

    pub fn device(&self) -> &VulkanDevice {
        unsafe {
            &*self.device_ptr
        }
    }

    pub fn alloc(&self, size: u64, map: bool) -> Result<MemoryBlock, VulkanUniversalError> {
        match unsafe {
            self.inner.lock().unwrap().alloc(self.memory_device(), Request {
                size,
                align_mask: 1,
                usage: if let true = map {
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

    fn memory_device(&self) -> &AshMemoryDevice {
        AshMemoryDevice::wrap(self.device().initialized().unwrap().vulkan_device())
    }
}

impl Drop for MemoryAllocator {
    fn drop(&mut self) {
        unsafe {
            self.inner.lock().unwrap().cleanup(self.memory_device());
        }
    }
}

pub(crate) struct MemoryBlock {
    allocator_ptr: *const MemoryAllocator,
    inner: Mutex<ManuallyDrop<gpu_alloc::MemoryBlock<vk::DeviceMemory>>>
}

impl MemoryBlock {
    fn new(allocator: &MemoryAllocator, inner: gpu_alloc::MemoryBlock<vk::DeviceMemory>) -> MemoryBlock {
        MemoryBlock {
            allocator_ptr: allocator,
            inner: Mutex::new(ManuallyDrop::new(inner))
        }
    }

    pub fn allocator(&self) -> &MemoryAllocator {
        unsafe {
            &*self.allocator_ptr
        }
    }

    pub fn memory(&self) -> vk::DeviceMemory {
        self.inner.lock().unwrap().memory().clone()
    }

    pub fn offset(&self) -> u64 {
        self.inner.lock().unwrap().offset()
    }

    pub fn read(&self, buffer: &mut [u8], start: u64) -> Result<(), VulkanUniversalError> {
        match unsafe {
            self.inner.lock().unwrap().read_bytes(self.allocator().memory_device(), start, buffer)
        } {
            Ok(()) => Ok(()),
            Err(err) => Err(err.into())
        }
    }

    pub fn write(&self, data: &[u8], start: u64) -> Result<(), VulkanUniversalError> {
        match unsafe {
            self.inner.lock().unwrap().write_bytes(self.allocator().memory_device(), start, data)
        } {
            Ok(()) => Ok(()),
            Err(err) => Err(err.into())
        }
    }
}

impl Drop for MemoryBlock {
    fn drop(&mut self) {
        let block = unsafe {
            ptr::read(self.inner.get_mut().unwrap() as &gpu_alloc::MemoryBlock<vk::DeviceMemory>)
        };

        unsafe {
            self.allocator().inner.lock().unwrap().dealloc(self.allocator().memory_device(), block)
        };
    }
}
