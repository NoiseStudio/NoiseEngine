use std::sync::Arc;

use vulkano::{instance::Instance, VulkanLibrary};

use crate::{
    interop::prelude::{InteropResult, ResultError, ResultErrorKind},
    graphics::vulkan::{vulkan_instance, vulkan_log_severity::VulkanLogSeverity, vulkan_log_type::VulkanLogType}
};

use super::vulkan_instance_create_info::VulkanInstanceCreateInfo;

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_create(
    library: &Arc<VulkanLibrary>, create_info: VulkanInstanceCreateInfo,
    log_severity: VulkanLogSeverity, log_type: VulkanLogType
) -> InteropResult<Box<Arc<Instance>>> {
    match vulkan_instance::create(library.clone(), create_info, log_severity, log_type) {
        Ok(instance) => InteropResult::with_ok(Box::new(instance)),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::GraphicsInstanceCreate))
        }
    }
}

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_destroy(_handle: Box<Arc<Instance>>) {
}
