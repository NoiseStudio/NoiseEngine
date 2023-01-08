use std::sync::Arc;

use crate::{
    rendering::vulkan::{render_pass::{RenderPass, RenderPassCreateInfo}, device::VulkanDevice},
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn rendering_vulkan_render_pass_create<'init>(
    device: &Arc<VulkanDevice<'init>>, create_info: RenderPassCreateInfo
) -> InteropResult<Box<RenderPass<'init>>> {
    match RenderPass::new(device, create_info) {
        Ok(r) => InteropResult::with_ok(Box::new(r)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_render_pass_destroy(_handle: Box<RenderPass>) {
}
