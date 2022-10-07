use std::{sync::Arc, ptr};

use vulkano::{instance::Instance, VulkanLibrary};

use crate::{interop::prelude::{InteropResult, ResultError, ResultErrorKind}, graphics::vulkan::vulkan_instance};

use super::vulkan_instance_create_info::VulkanInstanceCreateInfo;

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_create(
    library: &Arc<VulkanLibrary>,
    create_info: VulkanInstanceCreateInfo
) -> InteropResult<*const Instance> {
    match vulkan_instance::create(library.clone(), create_info) {
        Ok(instance) => InteropResult::with_ok(Arc::into_raw(instance)),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::GraphicsInstanceCreate))
        }
    }
}

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_destroy(instance: *mut Instance) {
    unsafe {
        ptr::drop_in_place(instance)
    }
}
