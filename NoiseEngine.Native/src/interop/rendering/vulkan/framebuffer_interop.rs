use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::{render_pass::RenderPass, framebuffer::{Framebuffer, FramebufferAttachment}},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan}
};

#[no_mangle]
extern "C" fn rendering_vulkan_framebuffer_create<'init: 'ma, 'ma>(
    render_pass: &Arc<RenderPass<'init>>, flags: vk::FramebufferCreateFlags, width: u32, height: u32, layers: u32,
    attachments: InteropReadOnlySpan<FramebufferAttachment<'init, 'ma>>
) -> InteropResult<Box<Framebuffer<'init, 'ma>>> {
    match Framebuffer::new(render_pass, flags, width, height, layers, attachments.into()) {
        Ok(f) => InteropResult::with_ok(Box::new(f)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_framebuffer_destroy(_handle: Box<Framebuffer>) {
}
