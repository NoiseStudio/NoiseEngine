use crate::{rendering::vulkan::device::VulkanDevice, interop::prelude::InteropResult};

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_destroy(_handle: Box<VulkanDevice>) {
}

/// # SAFETY
/// This function must be synchronized by caller.
#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_initialize(device: &mut VulkanDevice) -> InteropResult<()> {
    match device.initialize() {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
