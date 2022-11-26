use crate::{
    rendering::vulkan::{shader_module::ShaderModule, device::VulkanDevice},
    interop::{interop_read_only_span::InteropReadOnlySpan, prelude::InteropResult}
};

#[no_mangle]
extern "C" fn rendering_vulkan_shader_module_create<'dev: 'init, 'init>(
    device: &'dev VulkanDevice<'_, 'init>, code: InteropReadOnlySpan<u8>
) -> InteropResult<Box<ShaderModule<'init>>> {
    match ShaderModule::new(device, code.into()) {
        Ok(s) => InteropResult::with_ok(Box::new(s)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_shader_module_destroy(_handle: Box<ShaderModule>) {
}
