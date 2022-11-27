use std::{ptr, mem::ManuallyDrop, rc::Rc};

use ash::vk::{self, QueueFlags};
use lockfree::stack::Stack;
use rsevents::{AutoResetEvent, Awaitable, EventState};

use crate::{errors::invalid_operation::InvalidOperationError, common::pool::{Pool, PoolItem}};

use super::{
    device_support::VulkanDeviceSupport, errors::universal::VulkanUniversalError, memory_allocator::MemoryAllocator,
    device_pool::VulkanDevicePool, pool_wrappers::VulkanCommandPool
};

pub struct VulkanDevice<'inst: 'init, 'init> {
    instance: &'inst ash::Instance,
    physical_device: vk::PhysicalDevice,
    initialized: Option<VulkanDeviceInitialized<'init>>
}

impl<'inst: 'init, 'init> VulkanDevice<'inst, 'init> {
    pub fn new(instance: &'inst ash::Instance, physical_device: vk::PhysicalDevice) -> Self {
        Self { instance, physical_device, initialized: None }
    }

    fn create_not_initialized_error() -> InvalidOperationError {
        InvalidOperationError::with_str("VulkanDevice is not initialized.")
    }

    pub fn initialize(&mut self) -> Result<(), VulkanUniversalError> {
        if self.initialized.is_some() {
            return Err(InvalidOperationError::with_str("VulkanDevice is already initialized.").into())
        }

        let (queue_create_infos, _queue_create_info_priorities) =
            self.create_queue_create_infos();
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
            Rc::new(self.instance().create_device(self.physical_device, &create_info, None)?)
        };

        let queue_families = Self::create_queue_families(self.instance, self.physical_device, device.clone());
        let instance = self.instance;

        self.initialized = Some(VulkanDeviceInitialized {
            device: device.clone(),
            queue_families: ManuallyDrop::new(queue_families),
            allocator: ManuallyDrop::new(MemoryAllocator::new(instance, device.clone(), self.physical_device())?),
            pool: VulkanDevicePool::new(device.clone()),
        });

        Ok(())
    }

    pub fn instance(&self) -> &ash::Instance {
        self.instance
    }

    pub fn physical_device(&self) -> vk::PhysicalDevice {
        self.physical_device
    }

    pub fn get_queue<'dev: 'init>(
        &'dev self,
        support: VulkanDeviceSupport,
    ) -> Result<VulkanQueue, InvalidOperationError> {
        match self.initialized()?.get_family(support) {
            Ok(family) => Ok(family.get_queue()),
            Err(err) => Err(err)
        }
    }

    pub(crate) fn initialized(&self) -> Result<&VulkanDeviceInitialized<'init>, InvalidOperationError> {
        match &self.initialized {
            Some(i) => Ok(i),
            None => Err(Self::create_not_initialized_error())
        }
    }

    fn create_queue_create_infos(&self) -> (Vec<vk::DeviceQueueCreateInfo>, Vec<f32>) {
        let queue_families = unsafe {
            self.instance().get_physical_device_queue_family_properties(self.physical_device)
        };

        let mut queue_create_infos = Vec::with_capacity(queue_families.len());

        let mut queues = Vec::new();
        for
            (queue_family_index, queue_family) in (0u32..).zip(queue_families.into_iter())
        {
            for _ in queues.len()..queue_family.queue_count as usize {
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

        (queue_create_infos, queues)
    }

    fn create_queue_families(
        instance: &'inst ash::Instance,
        physical_device: vk::PhysicalDevice,
        device: Rc<ash::Device>
    ) -> Vec<VulkanQueueFamily<'init>> {
        let mut queue_family_index = 0;
        let mut families: Vec<_> = unsafe {
            instance.get_physical_device_queue_family_properties(physical_device)
        }.into_iter().map(|family| {
            let result = VulkanQueueFamily {
                vulkan_device: device.clone(),
                index: queue_family_index,
                support: VulkanDeviceSupport {
                    graphics: family.queue_flags.contains(QueueFlags::GRAPHICS),
                    computing: family.queue_flags.contains(QueueFlags::COMPUTE),
                    transfer: family.queue_flags.contains(QueueFlags::TRANSFER)
                },
                queues: Stack::new(),
                reset_event: AutoResetEvent::new(EventState::Set),
                command_pools: Pool::default()
            };

            for i in 0..family.queue_count {
                result.push_queue(unsafe {
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

pub(crate) struct VulkanDeviceInitialized<'init> {
    device: Rc<ash::Device>,
    queue_families: ManuallyDrop<Vec<VulkanQueueFamily<'init>>>,
    allocator: ManuallyDrop<MemoryAllocator>,
    pool: VulkanDevicePool<'init>,
}

impl<'init> VulkanDeviceInitialized<'init> {
    pub fn vulkan_device(&self) -> &ash::Device {
        &self.device
    }

    pub fn allocator(&self) -> &MemoryAllocator {
        &self.allocator
    }

    pub fn pool(&self) -> &VulkanDevicePool<'init> {
        &self.pool
    }

    pub fn queue_families_count(&self) -> usize {
        self.queue_families.len()
    }

    pub fn get_family(&self, support: VulkanDeviceSupport) -> Result<&VulkanQueueFamily<'init>, InvalidOperationError> {
        for family in &*self.queue_families {
            if support.is_suitable_to(&family.support) {
                return Ok(&family);
            }
        }

        Err(InvalidOperationError::with_str("This VulkanDevice does not have suitable families."))
    }
}

impl Drop for VulkanDeviceInitialized<'_> {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.queue_families);
            ManuallyDrop::drop(&mut self.allocator);
            self.device.destroy_device(None)
        }
    }
}

pub struct VulkanQueueFamily<'fam> {
    vulkan_device: Rc<ash::Device>,
    index: u32,
    support: VulkanDeviceSupport,
    queues: Stack<vk::Queue>,
    reset_event: AutoResetEvent,
    command_pools: Pool<VulkanCommandPool<'fam>>,
}

impl<'fam> VulkanQueueFamily<'fam> {
    pub fn index(&self) -> u32 {
        self.index
    }

    pub fn support(&self) -> &VulkanDeviceSupport {
        &self.support
    }

    pub fn try_get_queue(&self) -> Option<VulkanQueue<'fam, '_>> {
        match self.queues.pop() {
            Some(queue) => Some(VulkanQueue {
                family: self,
                queue
            }),
            None => None
        }
    }

    pub fn get_queue(&self) -> VulkanQueue<'fam, '_> {
        loop {
            match self.try_get_queue() {
                Some(queue) => return queue,
                None => self.reset_event.wait()
            }
        }
    }

    pub fn get_command_pool(&'fam self) -> Result<PoolItem<'fam, VulkanCommandPool<'fam>>, VulkanUniversalError> {
        self.command_pools.get_or_create(|| {
            let pool_info = vk::CommandPoolCreateInfo {
                s_type: vk::StructureType::COMMAND_POOL_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::CommandPoolCreateFlags::empty(),
                queue_family_index: self.index(),
            };

            let command_pool = unsafe {
                self.vulkan_device.create_command_pool(&pool_info, None)
            }?;

            Ok(VulkanCommandPool::new(&self.vulkan_device, command_pool))
        })
    }

    fn push_queue(&self, queue: vk::Queue) {
        self.queues.push(queue);
        self.reset_event.set();
    }
}

pub struct VulkanQueue<'init: 'fam, 'fam> {
    pub family: &'fam VulkanQueueFamily<'init>,
    pub queue: vk::Queue
}

impl Drop for VulkanQueue<'_, '_> {
    fn drop(&mut self) {
        self.family.push_queue(self.queue)
    }
}
