use noise_engine_native::{
    rendering::vulkan::{device::VulkanDevice, device_support::VulkanDeviceSupport},
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn rendering_vulkan_device_test_get_queue(device: &VulkanDevice) -> InteropResult<()> {
    match device.get_queue(VulkanDeviceSupport { graphics: false, computing: false, transfer: false }) {
        Ok(_) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
