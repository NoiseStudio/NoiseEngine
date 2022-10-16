use crate::{graphics::vulkan::device::VulkanDevice, interop::prelude::InteropResult};

#[no_mangle]
extern "C" fn graphics_vulkan_device_interop_destroy(_handle: Box<VulkanDevice>) {
}

#[no_mangle]
extern "C" fn graphics_vulkan_device_interop_initialize(device: &mut VulkanDevice) -> InteropResult<()> {
    match device.initialize() {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
