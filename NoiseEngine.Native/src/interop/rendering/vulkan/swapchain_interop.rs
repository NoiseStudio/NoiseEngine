use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::{vulkan::{swapchain::Swapchain, device::VulkanDevice}, presentation::window::Window},
    interop::prelude::InteropResult
};

#[repr(C)]
struct SwapchainCreateReturnValue<'init: 'fam, 'fam> {
    pub handle: Box<Arc<Swapchain<'init, 'fam>>>,
    pub format: vk::Format
}

#[no_mangle]
extern "C" fn rendering_vulkan_swapchain_interop_create<'init: 'fam, 'fam>(
    device: &'init Arc<VulkanDevice<'init>>, window: &Arc<dyn Window>
) -> InteropResult<SwapchainCreateReturnValue<'init, 'fam>> {
    let surface = match window.create_vulkan_surface(device.instance()) {
        Ok(s) => s,
        Err(err) => return InteropResult::with_err(err.into())
    };

    match Swapchain::new(device, surface) {
        Ok(s) => {
            let format = s.format().format;
            InteropResult::with_ok(SwapchainCreateReturnValue {
                handle: Box::new(s),
                format
            })
        },
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_swapchain_interop_destroy(_handle: Box<Arc<Swapchain>>) {
}
