use crate::{
    rendering::{vulkan::{
        device::VulkanDevice, device_support::VulkanDeviceSupport, buffers::command_buffer::VulkanCommandBuffer
    }, buffers::command_buffer::GraphicsCommandBuffer},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan},
    serialization::reader::SerializationReader
};

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_destroy(_handle: Box<VulkanDevice>) {
}

/// # SAFETY
/// This function must be synchronized by caller.
#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_initialize(device: &mut VulkanDevice) -> InteropResult<()> {
    match device.initialize() {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_device_interop_create_command_buffer<'a>(
    device: &'a VulkanDevice, data: InteropReadOnlySpan<u8>, usage: VulkanDeviceSupport, simultaneous_execute: bool
) -> InteropResult<Box<Box<dyn GraphicsCommandBuffer + 'a>>>{
    match VulkanCommandBuffer::new(
        device, SerializationReader::new(data.into()), usage, simultaneous_execute
    ) {
        Ok(command_buffer) => InteropResult::with_ok(Box::new(Box::new(command_buffer))),
        Err(err) => InteropResult::with_err(err.into())
    }
}
