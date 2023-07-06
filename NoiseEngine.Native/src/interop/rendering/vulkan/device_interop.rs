use std::sync::Arc;

use crate::{
    interop::{
        interop_read_only_span::InteropReadOnlySpan,
        prelude::{InteropResult, InteropString},
    },
    rendering::{
        buffers::command_buffer::GraphicsCommandBuffer,
        vulkan::{
            buffers::command_buffer::VulkanCommandBuffer, device::VulkanDevice,
            device_support::VulkanDeviceSupport,
        },
    },
    serialization::reader::SerializationReader,
};

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_destroy(_handle: Box<Arc<VulkanDevice>>) {}

/// # SAFETY
/// This function must be synchronized by caller.
#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_initialize(
    device: &Arc<VulkanDevice>,
    enabled_extensions: InteropReadOnlySpan<InteropString>,
) -> InteropResult<()> {
    let reference = unsafe { &mut *(Arc::as_ptr(device) as *mut VulkanDevice) };

    match reference.initialize(enabled_extensions.into()) {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_create_command_buffer<
    'dev: 'init,
    'init: 'cbuf,
    'cbuf,
>(
    device: &'dev Arc<VulkanDevice<'init>>,
    data: InteropReadOnlySpan<u8>,
    usage: VulkanDeviceSupport,
    simultaneous_execute: bool,
) -> InteropResult<Box<Box<dyn GraphicsCommandBuffer<'init> + 'cbuf>>> {
    match VulkanCommandBuffer::new(
        device,
        SerializationReader::new(data.into()),
        usage,
        simultaneous_execute,
    ) {
        Ok(command_buffer) => InteropResult::with_ok(Box::new(Box::new(command_buffer))),
        Err(err) => InteropResult::with_err(err.into()),
    }
}
