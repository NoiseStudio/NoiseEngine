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
    device: &'init Arc<VulkanDevice<'init>>, window: &Arc<dyn Window>, target_min_image_count: u32
) -> InteropResult<SwapchainCreateReturnValue<'init, 'fam>> {
    let surface = match window.create_vulkan_surface(device.instance()) {
        Ok(s) => s,
        Err(err) => return InteropResult::with_err(err.into())
    };

    match Swapchain::new(device, surface, target_min_image_count) {
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

#[no_mangle]
extern "C" fn rendering_vulkan_swapchain_interop_change_min_image_count(
    swapchain: &Arc<Swapchain>, target_count: u32
) -> InteropResult<u32> {
    match swapchain.change_min_image_count(target_count) {
        Ok(c) => InteropResult::with_ok(c),
        Err(err) => InteropResult::with_err(err.into())
    }
}
