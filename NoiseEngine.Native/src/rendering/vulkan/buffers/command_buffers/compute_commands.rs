use crate::{
    rendering::vulkan::buffers::command_buffer::VulkanCommandBuffer,
    serialization::reader::SerializationReader,
};

pub fn dispatch(
    data: &mut SerializationReader,
    buffer: &VulkanCommandBuffer,
    vulkan_device: &ash::Device,
) {
    unsafe {
        vulkan_device.cmd_dispatch(
            buffer.inner(),
            data.read_unchecked::<u32>(),
            data.read_unchecked::<u32>(),
            data.read_unchecked::<u32>(),
        );
    }
}
