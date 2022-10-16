use std::sync::Arc;

use lockfree::stack::Stack;
use rsevents::{AutoResetEvent, Awaitable, EventState};
use vulkano::device::{physical::PhysicalDevice, Device, DeviceCreateInfo, QueueCreateInfo, Queue};

use crate::errors::invalid_operation::InvalidOperationError;

use super::{errors::device_create::VulkanDeviceCreateError, device_support::VulkanDeviceSupport};

pub struct VulkanDevice {
    physical_device: Arc<PhysicalDevice>,
    initialized: Option<VulkanDeviceInitialized>
}

impl VulkanDevice {
    pub fn new(physical_device: Arc<PhysicalDevice>) -> Self {
        Self { physical_device, initialized: None }
    }

    fn create_not_initialized_error() -> InvalidOperationError{
        return InvalidOperationError::with_str("VulkanDevice is not initialized.")
    }

    pub fn initialize(&mut self) -> Result<(), VulkanDeviceCreateError> {
        if self.initialized.is_some() {
            return Err(InvalidOperationError::with_str("VulkanDevice is already initialized.").into())
        }

        let (device, queues) = match Device::new(
            self.physical_device().clone(),
            DeviceCreateInfo {
                queue_create_infos: self.create_queue_create_infos(),
                ..Default::default()
            }
        ) {
            Ok((device, queues)) => (device, queues),
            Err(err) => return Err(err.into()),
        };

        self.initialized = Some(VulkanDeviceInitialized {
            device,
            queues_families: self.create_queue_families(queues)
        });

        Ok(())
    }

    pub fn physical_device(&self) -> &Arc<PhysicalDevice> {
        &self.physical_device
    }

    pub fn device(&self) -> Result<&Arc<Device>, InvalidOperationError> {
        match &self.initialized {
            Some(i) => Ok(&i.device),
            None => Err(Self::create_not_initialized_error())
        }
    }

    pub fn get_queue(&self, support: VulkanDeviceSupport) -> Result<VulkanQueue, InvalidOperationError> {
        let initialized = match &self.initialized {
            Some(i) => i,
            None => return Err(Self::create_not_initialized_error())
        };

        for family in &initialized.queues_families {
            if support.is_suitable_to(&family.support) {
                return Ok(family.pop())
            }
        }

        Err(InvalidOperationError::with_str("This VulkanDevice does not have suitable queues."))
    }

    fn create_queue_create_infos(&self) -> Vec<QueueCreateInfo> {
        let queue_families = self.physical_device().queue_family_properties();

        let mut queue_create_infos = Vec::with_capacity(queue_families.len());
        let mut queue_family_index: u32 = 0;
        
        for queue_family in queue_families {
            let mut queues = Vec::new();
            for _ in 0..queue_family.queue_count {
                queues.push(1.0)
            }

            queue_create_infos.push(QueueCreateInfo {
                queue_family_index,
                queues,
                ..Default::default()
            });

            queue_family_index += 1;
        }

        queue_create_infos
    }

    fn create_queue_families(&self, queues: impl ExactSizeIterator<Item = Arc<Queue>>) -> Vec<VulkanQueueFamily> {
        let mut families: Vec<_> = self.physical_device()
            .queue_family_properties().into_iter().map(|family| VulkanQueueFamily {
                support: VulkanDeviceSupport {
                    graphics: family.queue_flags.graphics,
                    computing: family.queue_flags.compute,
                    transfer: family.queue_flags.transfer
                },
                queues: Stack::new(),
                reset_event: AutoResetEvent::new(EventState::Set)
        }).collect();

        for queue in queues {
            let family = &families[queue.queue_family_index() as usize];
            family.push(queue);
        }

        families.sort_by(|a, b| a.support.family_cmp(&b.support));
        families
    }
}

struct VulkanDeviceInitialized {
    device: Arc<Device>,
    queues_families: Vec<VulkanQueueFamily>
}

pub struct VulkanQueueFamily {
    support: VulkanDeviceSupport,
    queues: Stack<Arc<Queue>>,
    reset_event: AutoResetEvent
}

impl VulkanQueueFamily {
    pub fn support(&self) -> &VulkanDeviceSupport {
        &self.support
    }

    fn push(&self, queue: Arc<Queue>) {
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

pub struct VulkanQueue {
    family: *const VulkanQueueFamily,
    queue: Arc<Queue>
}

impl VulkanQueue {
    pub fn queue(&self) -> &Arc<Queue> {
        &self.queue
    }

    pub fn family(&self) -> &VulkanQueueFamily {
        unsafe {
            self.family.as_ref()
        }.unwrap()
    }
}

impl Drop for VulkanQueue {
    fn drop(&mut self) {
        self.family().push(self.queue.clone())
    }
}
