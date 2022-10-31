use std::ptr;

use ash::vk;

use crate::{
    rendering::{
        vulkan::{
            device::{VulkanDevice, VulkanQueueFamily}, device_support::VulkanDeviceSupport,
            errors::universal::VulkanUniversalError, fence::VulkanFence, pool_wrappers::VulkanCommandPool
        },
        buffers::{command_buffers::command::GraphicsCommandBufferCommand, command_buffer::GraphicsCommandBuffer},
        fence::GraphicsFence
    },
    serialization::reader::SerializationReader, interop::prelude::InteropResult, common::pool::PoolItem
};

use super::buffer::VulkanBuffer;

pub struct VulkanCommandBuffer<'a> {
    device: &'a VulkanDevice,
    inner: vk::CommandBuffer,
    queue_family: &'a VulkanQueueFamily,
    _command_pool: PoolItem<'a, VulkanCommandPool>,
}

impl<'a> VulkanCommandBuffer<'a> {
    pub fn new(
        device: &'a VulkanDevice, data: SerializationReader, usage: VulkanDeviceSupport, simultaneous_execute: bool
    ) -> Result<Self, VulkanUniversalError> {
        let queue_family = match device.get_family(usage) {
            Ok(family) => family,
            Err(err) => return Err(err.into())
        };

        let command_pool = queue_family.get_command_pool()?;

        let initialized = device.initialized()?;
        let vulkan_device = initialized.vulkan_device();

        let allocate_info = vk::CommandBufferAllocateInfo {
            s_type: vk::StructureType::COMMAND_BUFFER_ALLOCATE_INFO,
            p_next: ptr::null(),
            command_pool: command_pool.inner(),
            level: vk::CommandBufferLevel::PRIMARY,
            command_buffer_count: 1,
        };

        let command_buffer = unsafe {
            vulkan_device.allocate_command_buffers(&allocate_info)
        }?[0];

        let mut result = VulkanCommandBuffer {
            device,
            inner: command_buffer,
            queue_family,
            _command_pool: command_pool,
        };

        result.record(data, simultaneous_execute)?;

        Ok(result)
    }

    pub fn execute(&self) -> Result<VulkanFence, VulkanUniversalError> {
        let initialized = self.device.initialized()?;
        let vulkan_device = initialized.vulkan_device();

        let submit_info = vk::SubmitInfo {
            s_type: vk::StructureType::SUBMIT_INFO,
            p_next: ptr::null(),
            wait_semaphore_count: 0,
            p_wait_semaphores: ptr::null(),
            p_wait_dst_stage_mask: ptr::null(),
            command_buffer_count: 1,
            p_command_buffers: &self.inner as *const vk::CommandBuffer,
            signal_semaphore_count: 0,
            p_signal_semaphores: ptr::null(),
        };

        let fence = initialized.pool().get_fence()?;
        unsafe {
            vulkan_device.queue_submit(
                self.queue_family.get_queue().queue, &[submit_info], fence.inner()
            )
        }?;

        Ok(fence)
    }

    fn record(
        &mut self, mut data: SerializationReader, simultaneous_execute: bool
    ) -> Result<(), VulkanUniversalError> {
        let initialized = self.device.initialized()?;
        let vulkan_device = initialized.vulkan_device();

        unsafe {
            let mut begin_info_flags = vk::CommandBufferUsageFlags::ONE_TIME_SUBMIT;
            if simultaneous_execute {
                begin_info_flags |= vk::CommandBufferUsageFlags::SIMULTANEOUS_USE;
            }

            let begin_info = vk::CommandBufferBeginInfo {
                s_type: vk::StructureType::COMMAND_BUFFER_BEGIN_INFO,
                p_next: ptr::null(),
                flags: begin_info_flags,
                p_inheritance_info: ptr::null(),
            };

            vulkan_device.begin_command_buffer(self.inner, &begin_info)
        }?;

        while let Some(command) = data.read::<GraphicsCommandBufferCommand>() {
            match command {
                GraphicsCommandBufferCommand::CopyBuffer => {
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
                            self.inner, source_buffer.inner(),
                            destination_buffer.inner(), &regions
                        )
                    };
                }
            };
        };

        unsafe {
            vulkan_device.end_command_buffer(self.inner)
        }?;

        Ok(())
    }
}

impl Drop for VulkanCommandBuffer<'_> {
    fn drop(&mut self) {
        let initialized = self.device.initialized().unwrap();

        unsafe {
            initialized.vulkan_device().reset_command_buffer(
                self.inner, vk::CommandBufferResetFlags::empty()
            )
        }.unwrap();
    }
}

impl GraphicsCommandBuffer for VulkanCommandBuffer<'_> {
    fn execute(&self) -> InteropResult<Box<Box<dyn GraphicsFence + '_>>> {
        match self.execute() {
            Ok(fence) => InteropResult::with_ok(Box::new(Box::new(fence))),
            Err(err) => InteropResult::with_err(err.into())
        }
    }
}
