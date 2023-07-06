use std::sync::Arc;

use noise_engine_native::{
    interop::prelude::InteropResult,
    rendering::vulkan::{device::VulkanDevice, device_support::VulkanDeviceSupport},
};

#[no_mangle]
extern "C" fn rendering_vulkan_device_test_get_queue<'dev: 'init, 'init>(
    device: &'dev Arc<VulkanDevice<'init>>,
) -> InteropResult<()> {
    match device.get_queue(VulkanDeviceSupport {
        graphics: false,
        computing: false,
        transfer: false,
    }) {
        Ok(_) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into()),
    }
}
