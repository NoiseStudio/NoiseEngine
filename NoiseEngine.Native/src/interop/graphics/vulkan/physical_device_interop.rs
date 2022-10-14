use std::sync::Arc;

use vulkano::device::physical::PhysicalDevice;

#[no_mangle]
extern "C" fn graphics_vulkan_physical_device_interop_destroy(_handle: Box<Arc<PhysicalDevice>>) {
}