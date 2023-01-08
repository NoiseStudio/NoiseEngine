use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::{buffers::{buffer::VulkanBuffer, command_buffer::VulkanCommandBuffer}, image::VulkanImage},
    serialization::reader::SerializationReader
};

pub fn copy_buffer(data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device) {
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

pub fn copy_texture_to_buffer(
    data: &mut SerializationReader, buffer: &VulkanCommandBuffer, vulkan_device: &ash::Device
) {
    let source_texture = data.read_unchecked::<&Arc<VulkanImage>>();
    let destination_buffer = data.read_unchecked::<vk::Buffer>();

    let mut regions = Vec::with_capacity(data.read_unchecked::<i32>() as usize);
    for _ in 0..regions.capacity() {
        regions.push(read_unchecked_buffer_image_copy(data));
    }

    unsafe {
        vulkan_device.cmd_copy_image_to_buffer(
            buffer.inner(), source_texture.inner(), source_texture.layout(),
            destination_buffer, &regions
        )
    };
}

fn read_unchecked_buffer_image_copy(data: &mut SerializationReader) -> vk::BufferImageCopy {
    vk::BufferImageCopy {
        buffer_offset: data.read_unchecked(),
        buffer_row_length: 0,
        buffer_image_height: 0,
        image_subresource: data.read_unchecked(),
        image_offset: data.read_unchecked(),
        image_extent: data.read_unchecked(),
    }
}
