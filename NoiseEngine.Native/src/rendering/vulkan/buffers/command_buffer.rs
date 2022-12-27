use std::{ptr, sync::Arc};

use ash::vk;

use crate::{
    rendering::{
        vulkan::{
            device::{VulkanDevice, VulkanQueueFamily, VulkanDeviceInitialized}, device_support::VulkanDeviceSupport,
            errors::universal::VulkanUniversalError, fence::VulkanFence, pool_wrappers::VulkanCommandPool
        },
        buffers::{command_buffers::command::GraphicsCommandBufferCommand, command_buffer::GraphicsCommandBuffer},
        fence::GraphicsFence
    },
    serialization::reader::SerializationReader, interop::prelude::InteropResult, common::pool::PoolItem
};

use super::command_buffers::{memory_commands, compute_commands};

pub struct VulkanCommandBuffer<'init: 'fam, 'fam> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::CommandBuffer,
    queue_family: &'fam VulkanQueueFamily<'init>,
    command_pool: PoolItem<'fam, VulkanCommandPool<'init>>,
    device: Arc<VulkanDevice<'init>>,
}

impl<'dev: 'init, 'init: 'fam, 'fam> VulkanCommandBuffer<'init, 'fam> {
    pub fn new(
        device: &'dev Arc<VulkanDevice<'init>>,
        data: SerializationReader,
        usage: VulkanDeviceSupport,
        simultaneous_execute: bool,
    ) -> Result<Self, VulkanUniversalError> {
        let initialized = device.initialized()?;
        let queue_family = initialized.get_family(usage)?;
        let command_pool = queue_family.get_command_pool()?;

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

        let result = VulkanCommandBuffer {
            initialized,
            inner: command_buffer,
            queue_family,
            command_pool,
            device: device.clone(),
        };

        result.record(data, simultaneous_execute)?;

        Ok(result)
    }

    pub fn inner(&self) -> vk::CommandBuffer {
        self.inner
    }

    pub fn execute(&self) -> Result<VulkanFence<'init>, VulkanUniversalError> {
        let initialized = self.initialized;
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

        let fence = initialized.pool().get_fence(&self.device)?;
        unsafe {
            vulkan_device.queue_submit(
                self.queue_family.get_queue().queue, &[submit_info], fence.inner()
            )
        }?;

        Ok(fence)
    }

    fn record(
        &self, mut data: SerializationReader, simultaneous_execute: bool
    ) -> Result<(), VulkanUniversalError> {
        let initialized = self.initialized;
        let vulkan_device = initialized.vulkan_device();

        let mut begin_info_flags = vk::CommandBufferUsageFlags::ONE_TIME_SUBMIT;
        if simultaneous_execute {
            begin_info_flags = vk::CommandBufferUsageFlags::SIMULTANEOUS_USE;
        }

        let begin_info = vk::CommandBufferBeginInfo {
            s_type: vk::StructureType::COMMAND_BUFFER_BEGIN_INFO,
            p_next: ptr::null(),
            flags: begin_info_flags,
            p_inheritance_info: ptr::null(),
        };

        unsafe {
            vulkan_device.begin_command_buffer(self.inner, &begin_info)
        }?;

        while let Some(command) = data.read::<GraphicsCommandBufferCommand>() {
            match command {
                GraphicsCommandBufferCommand::CopyBuffer =>
                    memory_commands::copy_buffer(&mut data, self, vulkan_device),
                GraphicsCommandBufferCommand::Dispatch =>
                    compute_commands::dispatch(&mut data, self, vulkan_device)
            };
        };

        unsafe {
            vulkan_device.end_command_buffer(self.inner)
        }?;

        Ok(())
    }
}

impl Drop for VulkanCommandBuffer<'_, '_> {
    fn drop(&mut self) {
        let initialized = self.initialized;

        unsafe {
            initialized.vulkan_device().reset_command_pool(
                self.command_pool.inner(), vk::CommandPoolResetFlags::empty()
            )
        }.unwrap();
    }
}

impl<'init> GraphicsCommandBuffer<'init> for VulkanCommandBuffer<'init, '_> {
    fn execute(&self) -> InteropResult<Box<Box<dyn GraphicsFence + 'init>>> {
        match self.execute() {
            Ok(fence) => InteropResult::with_ok(Box::new(Box::new(fence))),
            Err(err) => InteropResult::with_err(err.into())
        }
    }
}
