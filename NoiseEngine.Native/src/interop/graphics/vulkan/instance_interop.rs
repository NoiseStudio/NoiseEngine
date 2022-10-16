use std::{sync::Arc, mem};

use enumflags2::BitFlags;
use uuid::Uuid;
use vulkano::{instance::Instance, VulkanLibrary};

use crate::{
    errors::overflow::OverflowError,
    graphics::{vulkan::{instance, log_severity::VulkanLogSeverity, log_type::VulkanLogType, device::VulkanDevice}},
    interop::prelude::{InteropResult, ResultError, ResultErrorKind, InteropArray}
};

use super::{instance_create_info::VulkanInstanceCreateInfo, device_value::VulkanDeviceValue};

#[no_mangle]
extern "C" fn graphics_vulkan_instance_interop_create(
    library: &Arc<VulkanLibrary>, create_info: VulkanInstanceCreateInfo,
    log_severity: BitFlags<VulkanLogSeverity>, log_type: BitFlags<VulkanLogType>
) -> InteropResult<Box<Arc<Instance>>> {
    match instance::create(library.clone(), create_info, log_severity, log_type) {
        Ok(instance) => InteropResult::with_ok(Box::new(instance)),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::GraphicsInstanceCreate))
        }
    }
}

#[no_mangle]
extern "C" fn graphics_vulkan_instance_interop_destroy(_handle: Box<Arc<Instance>>) {
}

#[no_mangle]
extern "C" fn graphics_vulkan_instance_interop_get_devices(
    instance: &Arc<Instance>
) -> InteropResult<InteropArray<VulkanDeviceValue>> {
    match instance.enumerate_physical_devices() {
        Ok(physical_devices) => {
            let mut result = Vec::with_capacity(physical_devices.len());

            for physical_device in physical_devices {
                let properties = physical_device.properties();

                let mut supports_graphics = false;
                let mut supports_computing = false;
                for queue_family_properties in physical_device.queue_family_properties() {
                    supports_graphics |= queue_family_properties.queue_flags.graphics;
                    supports_computing |= queue_family_properties.queue_flags.compute;
                }

                result.push(VulkanDeviceValue {
                    name: properties.device_name.to_owned().into(),
                    vendor: properties.vendor_id,
                    device_type: unsafe { mem::transmute(properties.device_type) },
                    api_version: match u32::try_from(properties.api_version) {
                        Ok(v) => v,
                        Err(_) => return InteropResult::with_err(OverflowError::with_str(
                            "Vulkan API version overflowed on physical device."
                        ).into())
                    },
                    driver_version: properties.driver_version,
                    guid: match properties.device_uuid {
                        Some(uuid) => Uuid::from_bytes_le(uuid),
                        None => Uuid::nil()
                    },
                    supports_graphics,
                    supports_computing,
                    handle: Box::new(VulkanDevice::new(physical_device))
                });
            }

            InteropResult::with_ok(result.into())
        },
        Err(err) => InteropResult::with_err(err.into())
    }
}
