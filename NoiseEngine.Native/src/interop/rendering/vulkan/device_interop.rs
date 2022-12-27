use std::sync::Arc;

use crate::{
    rendering::{vulkan::{
        device::VulkanDevice, device_support::VulkanDeviceSupport, buffers::command_buffer::VulkanCommandBuffer
    }, buffers::command_buffer::GraphicsCommandBuffer},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan},
    serialization::reader::SerializationReader
};

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_destroy(_handle: Box<Arc<VulkanDevice>>) {
}

/// # SAFETY
/// This function must be synchronized by caller.
#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_initialize(device: &Arc<VulkanDevice>) -> InteropResult<()> {
    let reference = unsafe {
        &mut *(Arc::as_ptr(device) as *mut VulkanDevice)
    };

    match reference.initialize() {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_create_command_buffer<'dev: 'init, 'init: 'cbuf, 'cbuf>(
    device: &'dev Arc<VulkanDevice<'init>>,
    data: InteropReadOnlySpan<u8>,
    usage: VulkanDeviceSupport,
    simultaneous_execute: bool,
) -> InteropResult<Box<Box<dyn GraphicsCommandBuffer<'init> + 'cbuf>>>{
    match VulkanCommandBuffer::new(
        device, SerializationReader::new(data.into()), usage, simultaneous_execute
    ) {
        Ok(command_buffer) => InteropResult::with_ok(Box::new(Box::new(command_buffer))),
        Err(err) => InteropResult::with_err(err.into())
    }
}
