use ash::vk;

use crate::{rendering::vulkan::instance, interop::prelude::{InteropResult, ResultError, ResultErrorKind, InteropArray}};

use super::{application_info::VulkanApplicationInfo, device_value::VulkanDeviceValue};

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_create(
    library: &ash::Entry, create_info: VulkanApplicationInfo,
    log_severity: vk::DebugUtilsMessageSeverityFlagsEXT, log_type: vk::DebugUtilsMessageTypeFlagsEXT
) -> InteropResult<Box<ash::Instance>> {
    match instance::create(library, create_info, log_severity, log_type) {
        Ok(instance) => InteropResult::with_ok(Box::new(instance)),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::GraphicsInstanceCreate))
        }
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_destroy(handle: Box<ash::Instance>) {
    unsafe { handle.destroy_instance(None); }
}

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_get_devices(
    instance: &ash::Instance
) -> InteropResult<InteropArray<VulkanDeviceValue>> {
    match instance::get_vulkan_physical_devices(instance) {
        Ok(devices) => InteropResult::with_ok(devices.into()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
