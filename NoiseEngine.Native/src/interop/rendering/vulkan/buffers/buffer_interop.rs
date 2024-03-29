use std::sync::Arc;

use ash::vk;

use crate::{
    interop::prelude::InteropResult,
    rendering::{
        buffers::buffer::GraphicsBuffer,
        vulkan::{buffers::buffer::VulkanBuffer, device::VulkanDevice},
    },
};

#[repr(C)]
struct VulkanBufferCreateReturnValue<'buf> {
    pub handle: Box<Box<dyn GraphicsBuffer + 'buf>>,
    pub inner_handle: vk::Buffer,
}

#[no_mangle]
extern "C" fn rendering_vulkan_buffers_buffer_interop_create<'dev: 'init, 'init: 'buf, 'buf>(
    device: &'dev Arc<VulkanDevice<'init>>,
    usage: vk::BufferUsageFlags,
    size: u64,
    map: bool,
) -> InteropResult<VulkanBufferCreateReturnValue<'buf>> {
    match VulkanBuffer::new(device, usage, size, map) {
        Ok(buffer) => {
            let inner = buffer.inner();
            InteropResult::with_ok(VulkanBufferCreateReturnValue {
                handle: Box::new(Box::new(buffer)),
                inner_handle: inner,
            })
        }
        Err(err) => InteropResult::with_err(err.into()),
    }
}
