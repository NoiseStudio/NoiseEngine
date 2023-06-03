use std::{ffi::CString, mem::ManuallyDrop, ptr, rc::Rc, sync::Arc};

use ash::vk::{self, QueueFlags};
use lockfree::stack::Stack;
use rsevents::{AutoResetEvent, Awaitable, EventState};

use crate::{
    common::pool::{Pool, PoolItem},
    errors::invalid_operation::InvalidOperationError,
    interop::prelude::InteropString,
    logging::log,
};

use super::{
    device_pool::VulkanDevicePool, device_support::VulkanDeviceSupport,
    errors::universal::VulkanUniversalError, instance::VulkanInstance,
    memory_allocator::MemoryAllocator, pool_wrappers::VulkanCommandPool,
};

pub struct VulkanDevice<'init> {
    initialized: Option<VulkanDeviceInitialized<'init>>,
    physical_device: vk::PhysicalDevice,
    instance: Arc<VulkanInstance>,
}

impl<'init> VulkanDevice<'init> {
    pub fn new(instance: &Arc<VulkanInstance>, physical_device: vk::PhysicalDevice) -> Self {
        Self {
            initialized: None,
            physical_device,
            instance: instance.clone(),
        }
    }

    fn create_not_initialized_error() -> InvalidOperationError {
        InvalidOperationError::with_str("VulkanDevice is not initialized.")
    }

    pub fn initialize(
        &mut self,
        enabled_extensions: &[InteropString],
    ) -> Result<(), VulkanUniversalError> {
        if self.initialized.is_some() {
            return Err(
                InvalidOperationError::with_str("VulkanDevice is already initialized.").into(),
            );
        }

        let (queue_create_infos, _queue_create_info_priorities) = self.create_queue_create_infos();
        let physical_device_features = vk::PhysicalDeviceFeatures {
            ..Default::default()
        };

        let mut enabled_extensions_c = Vec::new();
        let mut enabled_extensions_result = Vec::new();

        for extension in enabled_extensions {
            let c = match CString::new(extension) {
                Ok(c) => c,
                Err(_) => {
                    return Err(InvalidOperationError::with_str(
                        "Extension name contains null character.",
                    )
                    .into())
                }
            };
            enabled_extensions_result.push(c.as_ptr());
            enabled_extensions_c.push(c);
        }

        let create_info = vk::DeviceCreateInfo {
            s_type: vk::StructureType::DEVICE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::DeviceCreateFlags::empty(),
            queue_create_info_count: queue_create_infos.len() as u32,
            p_queue_create_infos: queue_create_infos.as_ptr(),
            enabled_layer_count: 0,
            pp_enabled_layer_names: ptr::null(),
            enabled_extension_count: enabled_extensions_result.len() as u32,
            pp_enabled_extension_names: enabled_extensions_result.as_ptr(),
            p_enabled_features: &physical_device_features,
        };

        let device = unsafe {
            Rc::new(self.instance().inner().create_device(
                self.physical_device,
                &create_info,
                None,
            )?)
        };

        let queue_families =
            Self::create_queue_families(&self.instance, self.physical_device, device.clone());

        self.initialized = Some(VulkanDeviceInitialized {
            device: device.clone(),
            queue_families: ManuallyDrop::new(queue_families),
            allocator: ManuallyDrop::new(MemoryAllocator::new(
                self.instance(),
                device.clone(),
                self.physical_device(),
            )?),
            pool: ManuallyDrop::new(VulkanDevicePool::new(device.clone())),
        });

        log::info(
            format!(
                "Initialized VulkanDevice {{ InnerHandle = {:p} }}.",
                device.handle()
            )
            .as_str(),
        );
        Ok(())
    }

    pub fn instance(&self) -> &Arc<VulkanInstance> {
        &self.instance
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
            Err(err) => Err(err),
        }
    }

    pub(crate) fn initialized(
        &self,
    ) -> Result<&VulkanDeviceInitialized<'init>, InvalidOperationError> {
        match &self.initialized {
            Some(i) => Ok(i),
            None => Err(Self::create_not_initialized_error()),
        }
    }

    fn create_queue_create_infos(&self) -> (Vec<vk::DeviceQueueCreateInfo>, Vec<f32>) {
        let queue_families = unsafe {
            self.instance()
                .inner()
                .get_physical_device_queue_family_properties(self.physical_device)
        };

        let mut queue_create_infos = Vec::with_capacity(queue_families.len());

        let mut queues = Vec::new();
        for (queue_family_index, queue_family) in (0u32..).zip(queue_families.into_iter()) {
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
        instance: &Arc<VulkanInstance>,
        physical_device: vk::PhysicalDevice,
        device: Rc<ash::Device>,
    ) -> Vec<VulkanQueueFamily<'init>> {
        let mut queue_family_index = 0;
        let mut families: Vec<_> = unsafe {
            instance
                .inner()
                .get_physical_device_queue_family_properties(physical_device)
        }
        .into_iter()
        .map(|family| {
            let result = VulkanQueueFamily {
                vulkan_device: device.clone(),
                index: queue_family_index,
                support: VulkanDeviceSupport {
                    graphics: family.queue_flags.contains(QueueFlags::GRAPHICS),
                    computing: family.queue_flags.contains(QueueFlags::COMPUTE),
                    transfer: family.queue_flags.contains(QueueFlags::TRANSFER),
                },
                queues: Stack::new(),
                reset_event: AutoResetEvent::new(EventState::Unset),
                command_pools: Pool::default(),
            };

            for i in 0..family.queue_count {
                result.push_queue(unsafe { device.get_device_queue(queue_family_index, i) });
            }

            queue_family_index += 1;

            result
        })
        .collect();

        families.sort_by(|a, b| a.support.family_cmp(&b.support));
        families
    }
}

pub(crate) struct VulkanDeviceInitialized<'init> {
    device: Rc<ash::Device>,
    queue_families: ManuallyDrop<Vec<VulkanQueueFamily<'init>>>,
    allocator: ManuallyDrop<MemoryAllocator>,
    pool: ManuallyDrop<VulkanDevicePool<'init>>,
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

    pub fn get_families(&self) -> &Vec<VulkanQueueFamily<'init>> {
        &self.queue_families
    }

    pub fn get_family(
        &self,
        support: VulkanDeviceSupport,
    ) -> Result<&VulkanQueueFamily<'init>, InvalidOperationError> {
        for family in &*self.queue_families {
            if support.is_suitable_to(&family.support) {
                return Ok(family);
            }
        }

        Err(InvalidOperationError::with_str(
            "This VulkanDevice does not have suitable families.",
        ))
    }
}

impl Drop for VulkanDeviceInitialized<'_> {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.pool);
            ManuallyDrop::drop(&mut self.queue_families);
            ManuallyDrop::drop(&mut self.allocator);

            self.device.destroy_device(None);
        }

        log::info(
            format!(
                "Dropped initialized VulkanDevice {{ InnerHandle = {:p} }}.",
                self.device.handle()
            )
            .as_str(),
        );
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
        self.queues.pop().map(|queue| VulkanQueue {
            family: self,
            queue,
        })
    }

    pub fn get_queue(&self) -> VulkanQueue<'fam, '_> {
        loop {
            match self.try_get_queue() {
                Some(queue) => return queue,
                None => self.reset_event.wait(),
            }
        }
    }

    pub fn get_command_pool(
        &'fam self,
    ) -> Result<PoolItem<'fam, VulkanCommandPool<'fam>>, VulkanUniversalError> {
        self.command_pools.get_or_create(|| {
            let pool_info = vk::CommandPoolCreateInfo {
                s_type: vk::StructureType::COMMAND_POOL_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::CommandPoolCreateFlags::empty(),
                queue_family_index: self.index(),
            };

            let command_pool = unsafe { self.vulkan_device.create_command_pool(&pool_info, None) }?;

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
    pub queue: vk::Queue,
}

impl Drop for VulkanQueue<'_, '_> {
    fn drop(&mut self) {
        self.family.push_queue(self.queue)
    }
}
