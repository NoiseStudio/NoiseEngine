use std::{sync::Arc, ptr};

use vulkano::instance::Instance;

use crate::{interop::prelude::{InteropResult, ResultError}, graphics::vulkan::vulkan_instance};

use super::vulkan_instance_create_info::VulkanInstanceCreateInfo;

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_create(
    create_info: VulkanInstanceCreateInfo
) -> InteropResult<*const Instance> {
    match vulkan_instance::create(create_info) {
        Ok(instance) => InteropResult::with_ok(Arc::into_raw(instance)),
        Err(err) => InteropResult::with_err(ResultError::new(&err))
    }
}

#[no_mangle]
extern "C" fn graphics_vulkan_vulkan_instance_interop_destroy(instance: *mut Instance) {
    unsafe {
        ptr::drop_in_place(instance)
    }
}

