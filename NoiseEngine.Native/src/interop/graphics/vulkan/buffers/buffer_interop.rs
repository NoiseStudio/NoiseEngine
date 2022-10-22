use enumflags2::BitFlags;
use vulkano::DeviceSize;

use crate::{
    graphics::{
        vulkan::{device::VulkanDevice, buffers::buffer::VulkanBuffer},
        buffers::{buffer_usage::GraphicsBufferUsage, buffer::GraphicsBuffer}
    },
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn graphics_vulkan_buffers_buffer_interop_create(
    device: &VulkanDevice, usage: BitFlags<GraphicsBufferUsage>, size: DeviceSize, map: bool
) -> InteropResult<Box<Box<dyn GraphicsBuffer + '_>>> {
    match VulkanBuffer::new(device, usage, size, map) {
        Ok(buffer) => InteropResult::with_ok(Box::new(Box::new(buffer))),
        Err(err) => InteropResult::with_err(err.into())
    }
}
