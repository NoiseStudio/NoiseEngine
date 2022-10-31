use ash::vk;

use crate::{
    rendering::vulkan::buffers::{buffer::VulkanBuffer, command_buffer::VulkanCommandBuffer},
    serialization::reader::SerializationReader
};

pub fn copy_buffer<'a>(data: &'a mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
    let source_buffer = data.read_unchecked::<&&VulkanBuffer>();
    let destination_buffer = data.read_unchecked::<&&VulkanBuffer>();

    let mut regions = Vec::with_capacity(data.read_unchecked::<i32>() as usize);
    for _ in 0..regions.capacity() {
        regions.push(vk::BufferCopy {
            src_offset: data.read_unchecked(),
            dst_offset: data.read_unchecked(),
            size: data.read_unchecked(),
        });
    }

    unsafe {
        vulkan_device.cmd_copy_buffer(
            buffer.inner(), source_buffer.inner(),
            destination_buffer.inner(), &regions
        )
    };
}
