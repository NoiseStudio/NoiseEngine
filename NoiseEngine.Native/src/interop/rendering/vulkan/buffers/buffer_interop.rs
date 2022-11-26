use ash::vk;

use crate::{
    rendering::{
        vulkan::{device::VulkanDevice, buffers::buffer::VulkanBuffer},
        buffers::buffer::GraphicsBuffer
    },
    interop::prelude::InteropResult
};

#[repr(C)]
struct VulkanBufferCreateReturnValue<'buf> {
    pub handle: Box<Box<dyn GraphicsBuffer + 'buf>>,
    pub inner_handle: vk::Buffer
}

#[no_mangle]
extern "C" fn rendering_vulkan_buffers_buffer_interop_create<'dev: 'init, 'init: 'buf, 'buf>(
    device: &'dev VulkanDevice<'_, 'init>, usage: vk::BufferUsageFlags, size: u64, map: bool
) -> InteropResult<VulkanBufferCreateReturnValue<'buf>> {
    let initialized = match device.initialized() {
        Ok(initialized) => initialized,
        Err(err) => return InteropResult::with_err(err.into())
    };

    match VulkanBuffer::new(initialized, usage, size, map) {
        Ok(buffer) => {
            let inner = buffer.inner();
            InteropResult::with_ok(VulkanBufferCreateReturnValue {
                handle: Box::new(Box::new(buffer)),
                inner_handle: inner
            })
        },
        Err(err) => InteropResult::with_err(err.into())
    }
}
