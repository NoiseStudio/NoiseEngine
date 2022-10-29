use std::{ptr, mem::ManuallyDrop};

use ash::vk::{self, QueueFlags};
use lockfree::stack::Stack;
use rsevents::{AutoResetEvent, Awaitable, EventState};

use crate::errors::invalid_operation::InvalidOperationError;

use super::{
    device_support::VulkanDeviceSupport, errors::universal::VulkanUniversalError, memory_allocator::MemoryAllocator
};

pub struct VulkanDevice {
    instance_ptr: *const ash::Instance,
    physical_device: vk::PhysicalDevice,
    initialized: Option<VulkanDeviceInitialized>
}

impl VulkanDevice {
    pub fn new(instance: &ash::Instance, physical_device: vk::PhysicalDevice) -> Self {
        Self { instance_ptr: instance, physical_device, initialized: None }
    }

    fn create_not_initialized_error() -> InvalidOperationError {
        InvalidOperationError::with_str("VulkanDevice is not initialized.")
    }

    pub fn initialize(&mut self) -> Result<(), VulkanUniversalError> {
        if self.initialized.is_some() {
            return Err(InvalidOperationError::with_str("VulkanDevice is already initialized.").into())
        }

        let queue_create_infos = self.create_queue_create_infos();
        let physical_device_features = vk::PhysicalDeviceFeatures {
            ..Default::default()
        };

        let create_info = vk::DeviceCreateInfo {
            s_type: vk::StructureType::DEVICE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::DeviceCreateFlags::empty(),
            queue_create_info_count: queue_create_infos.len() as u32,
            p_queue_create_infos: queue_create_infos.as_ptr(),
            enabled_layer_count: 0,
            pp_enabled_layer_names: ptr::null(),
            enabled_extension_count: 0,
            pp_enabled_extension_names: ptr::null(),
            p_enabled_features: &physical_device_features,
        };

        let device = unsafe {
            self.instance().create_device(self.physical_device, &create_info, None)
        }?;

        let queue_families = self.create_queue_families(&device);

        self.initialized = Some(VulkanDeviceInitialized {
            device,
            queue_families,
            allocator: ManuallyDrop::new(MemoryAllocator::new(self)?)
        });

        Ok(())
    }

    pub fn instance(&self) -> &ash::Instance {
        unsafe {
            &*self.instance_ptr
        }
    }

    pub fn physical_device(&self) -> vk::PhysicalDevice {
        self.physical_device
    }

    pub fn get_queue(&self, support: VulkanDeviceSupport) -> Result<VulkanQueue, InvalidOperationError> {
        let initialized = match &self.initialized {
            Some(i) => i,
            None => return Err(Self::create_not_initialized_error())
        };

        for family in &initialized.queue_families {
            if support.is_suitable_to(&family.support) {
                return Ok(family.pop())
            }
        }

        Err(InvalidOperationError::with_str("This VulkanDevice does not have suitable queues."))
    }

    pub(crate) fn initialized(&self) -> Result<&VulkanDeviceInitialized, InvalidOperationError> {
        match &self.initialized {
            Some(i) => Ok(i),
            None => Err(Self::create_not_initialized_error())
        }
    }

    fn create_queue_create_infos(&self) -> Vec<vk::DeviceQueueCreateInfo> {
        let queue_families = unsafe {
            self.instance().get_physical_device_queue_family_properties(self.physical_device)
        };

        let mut queue_create_infos = Vec::with_capacity(queue_families.len());

        for
            (queue_family_index, queue_family) in (0_u32..).zip(queue_families.into_iter())
        {
            let mut queues = Vec::new();
            for _ in 0..queue_family.queue_count {
                queues.push(1.0)
            }

            queue_create_infos.push(vk::DeviceQueueCreateInfo {
                s_type: vk::StructureType::DEVICE_QUEUE_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::DeviceQueueCreateFlags::empty(),
                queue_family_index,
                queue_count: queue_family.queue_count,
                p_queue_priorities: queues.as_ptr(),
            });
        }

        queue_create_infos
    }

    fn create_queue_families(&self, device: &ash::Device) -> Vec<VulkanQueueFamily> {
        let mut queue_family_index = 0;
        let mut families: Vec<_> = unsafe {
            self.instance().get_physical_device_queue_family_properties(self.physical_device)
        }.into_iter().map(|family| {
            let result = VulkanQueueFamily {
                support: VulkanDeviceSupport {
                    graphics: family.queue_flags.contains(QueueFlags::GRAPHICS),
                    computing: family.queue_flags.contains(QueueFlags::COMPUTE),
                    transfer: family.queue_flags.contains(QueueFlags::TRANSFER)
                },
                queues: Stack::new(),
                reset_event: AutoResetEvent::new(EventState::Set)
            };

            for i in 0..family.queue_count {
                result.push(unsafe {
                    device.get_device_queue(queue_family_index, i)
                });
            }

            queue_family_index += 1;

            result
        }).collect();

        families.sort_by(|a, b| a.support.family_cmp(&b.support));
        families
    }
}

pub(crate) struct VulkanDeviceInitialized {
    device: ash::Device,
    queue_families: Vec<VulkanQueueFamily>,
    allocator: ManuallyDrop<MemoryAllocator>
}

impl VulkanDeviceInitialized {
    pub fn vulkan_device(&self) -> &ash::Device {
        &self.device
    }

    pub fn allocator(&self) -> &MemoryAllocator {
        &self.allocator
    }

    pub fn queue_families_count(&self) -> usize {
        self.queue_families.len()
    }
}

impl Drop for VulkanDeviceInitialized {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.allocator);
            self.device.destroy_device(None)
        }
    }
}

pub struct VulkanQueueFamily {
    support: VulkanDeviceSupport,
    queues: Stack<vk::Queue>,
    reset_event: AutoResetEvent
}

impl VulkanQueueFamily {
    pub fn support(&self) -> &VulkanDeviceSupport {
        &self.support
    }

    fn push(&self, queue: vk::Queue) {
        self.queues.push(queue);
        self.reset_event.set();
    }

    fn pop(&self) -> VulkanQueue {
        loop {
            match self.queues.pop() {
                Some(queue) => return VulkanQueue {
                    family: self,
                    queue
                },
                None => self.reset_event.wait()
            }
        }
    }
}

pub struct VulkanQueue<'a> {
    pub family: &'a VulkanQueueFamily,
    pub queue: vk::Queue
}

impl<'a> Drop for VulkanQueue<'a> {
    fn drop(&mut self) {
        self.family.push(self.queue)
    }
}
