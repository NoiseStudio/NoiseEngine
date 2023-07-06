use std::sync::Arc;

use crate::{
    interop::{interop_read_only_span::InteropReadOnlySpan, prelude::InteropResult},
    rendering::vulkan::{device::VulkanDevice, shader_module::ShaderModule},
};

#[no_mangle]
extern "C" fn rendering_vulkan_shader_module_create<'dev: 'init, 'init>(
    device: &'dev Arc<VulkanDevice<'init>>,
    code: InteropReadOnlySpan<u8>,
) -> InteropResult<Box<ShaderModule<'init>>> {
    match ShaderModule::new(device, code.into()) {
        Ok(s) => InteropResult::with_ok(Box::new(s)),
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_shader_module_destroy(_handle: Box<ShaderModule>) {}
