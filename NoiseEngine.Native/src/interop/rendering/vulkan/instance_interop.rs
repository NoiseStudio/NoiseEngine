use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::instance::VulkanInstance,
    interop::prelude::{InteropResult, ResultError, ResultErrorKind, InteropArray}
};

use super::{application_info::VulkanApplicationInfo, device_value::VulkanDeviceValue};

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_create(
    library: &Arc<ash::Entry>, create_info: VulkanApplicationInfo, log_severity: vk::DebugUtilsMessageSeverityFlagsEXT,
    log_type: vk::DebugUtilsMessageTypeFlagsEXT, validation: bool
) -> InteropResult<Box<Arc<VulkanInstance>>> {
    match VulkanInstance::new(library, create_info, log_severity, log_type, validation) {
        Ok(instance) => InteropResult::with_ok(Box::new(Arc::new(instance))),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::GraphicsInstanceCreate))
        }
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_destroy(_handle: Box<Arc<VulkanInstance>>) {
}

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_get_devices(
    instance: &Arc<VulkanInstance>
) -> InteropResult<InteropArray<VulkanDeviceValue>> {
    match VulkanInstance::get_vulkan_physical_devices(instance) {
        Ok(devices) => InteropResult::with_ok(devices.into()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
