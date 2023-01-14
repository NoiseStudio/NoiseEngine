use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::instance::VulkanInstance,
    interop::{prelude::{InteropResult, ResultError, ResultErrorKind, InteropArray, InteropString}, interop_read_only_span::InteropReadOnlySpan}
};

use super::{application_info::VulkanApplicationInfo, device_value::VulkanDeviceValue};

#[repr(C)]
struct VulkanInstanceCreateReturnValue {
    pub handle: Box<Arc<VulkanInstance>>,
    pub inner_handle: vk::Instance
}

#[no_mangle]
extern "C" fn rendering_vulkan_instance_interop_create(
    library: &Arc<ash::Entry>, create_info: VulkanApplicationInfo, log_severity: vk::DebugUtilsMessageSeverityFlagsEXT,
    log_type: vk::DebugUtilsMessageTypeFlagsEXT, validation: bool,
    enabled_extensions: InteropReadOnlySpan<InteropString>
) -> InteropResult<VulkanInstanceCreateReturnValue> {
    match VulkanInstance::new(
        library, create_info, log_severity, log_type, validation,
        enabled_extensions.into()
    ) {
        Ok(instance) => {
            let inner = instance.inner().handle();
            InteropResult::with_ok(VulkanInstanceCreateReturnValue {
                handle: Box::new(Arc::new(instance)),
                inner_handle: inner
            })
        },
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
