use std::sync::Arc;

use vulkano::{device::{physical::PhysicalDevice, Device, DeviceCreateInfo, QueueCreateInfo}};

use crate::{errors::invalid_operation::InvalidOperationError};

use super::errors::device_create::VulkanDeviceCreateError;

#[repr(C)]
pub(crate) struct VulkanDevice {
    physical_device: Arc<PhysicalDevice>,
    device: Option<Arc<Device>>
}

impl VulkanDevice {
    pub fn new(physical_device: Arc<PhysicalDevice>) -> Self {
        Self { physical_device, device: None }
    }

    pub fn initialize(&mut self) -> Result<(), VulkanDeviceCreateError> {
        if self.device.is_some() {
            return Err(InvalidOperationError::with_str("VulkanDevice is already initialized.").into())
        }

        // TODO: Add queues management.
        let (device, _queues) = match Device::new(
            self.physical_device(),
            DeviceCreateInfo {
                queue_create_infos: self.create_queue_create_infos(),
                ..Default::default()
            }
        ) {
            Ok((device, queues)) => (device, queues),
            Err(err) => return Err(err.into()),
        };

        self.device = Some(device);
        Ok(())
    }

    pub fn physical_device(&self) -> Arc<PhysicalDevice> {
        self.physical_device.clone()
    }

    fn create_queue_create_infos(&self) -> Vec<QueueCreateInfo> {
        let physical_device = self.physical_device();
        let queue_families = physical_device.queue_family_properties();

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
}
