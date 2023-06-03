use std::{
    ptr,
    sync::{Arc, Weak},
};

use ash::vk;

use crate::{
    common::pool::PoolItem,
    interop::prelude::InteropResult,
    rendering::{
        buffers::{
            command_buffer::GraphicsCommandBuffer,
            command_buffers::command::GraphicsCommandBufferCommand,
        },
        fence::GraphicsFence,
        vulkan::{
            device::{VulkanDevice, VulkanDeviceInitialized, VulkanQueueFamily},
            device_support::VulkanDeviceSupport,
            errors::universal::VulkanUniversalError,
            fence::VulkanFence,
            pool_wrappers::VulkanCommandPool,
        },
    },
    serialization::reader::SerializationReader,
};

use super::command_buffers::{
    camera_commands::{self, AttachCameraWindowOutput},
    compute_commands, draw_commands, memory_commands, misc_commands,
};

pub struct VulkanCommandBuffer<'init: 'fam, 'fam> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::CommandBuffer,
    queue_family: &'fam VulkanQueueFamily<'init>,
    command_pool: PoolItem<'fam, VulkanCommandPool<'init>>,
    used_fence: Option<Arc<VulkanFence<'init>>>,
    attached_camera_windows: Vec<AttachCameraWindowOutput<'init, 'fam>>,
    attached_pipeline_layout: (vk::PipelineLayout, vk::PipelineBindPoint),
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

        let command_buffer = unsafe { vulkan_device.allocate_command_buffers(&allocate_info) }?[0];

        let mut result = VulkanCommandBuffer {
            initialized,
            inner: command_buffer,
            queue_family,
            command_pool,
            used_fence: None,
            attached_camera_windows: Vec::new(),
            attached_pipeline_layout: (vk::PipelineLayout::null(), vk::PipelineBindPoint::GRAPHICS),
            device: device.clone(),
        };

        result.record(data, simultaneous_execute)?;

        Ok(result)
    }

    pub fn inner(&self) -> vk::CommandBuffer {
        self.inner
    }

    pub fn attached_pipeline_layout(&self) -> (vk::PipelineLayout, vk::PipelineBindPoint) {
        self.attached_pipeline_layout
    }

    pub fn execute(&self) -> Result<Arc<VulkanFence<'init>>, VulkanUniversalError> {
        let initialized = self.initialized;
        let vulkan_device = initialized.vulkan_device();

        let mut wait_semaphores = Vec::new();
        let mut wait_stages = Vec::new();
        let mut signal_semaphores = Vec::new();

        for output in &self.attached_camera_windows {
            wait_semaphores.push(
                output
                    .pass
                    .get_image_available_semaphore(output.frame_index)
                    .inner(),
            );
            wait_stages.push(vk::PipelineStageFlags::COLOR_ATTACHMENT_OUTPUT);
            signal_semaphores.push(
                output
                    .pass
                    .get_render_finished_semaphore(output.frame_index)
                    .inner(),
            );
        }

        let submit_info = vk::SubmitInfo {
            s_type: vk::StructureType::SUBMIT_INFO,
            p_next: ptr::null(),
            wait_semaphore_count: wait_semaphores.len() as u32,
            p_wait_semaphores: wait_semaphores.as_ptr(),
            p_wait_dst_stage_mask: wait_stages.as_ptr(),
            command_buffer_count: 1,
            p_command_buffers: &self.inner as *const vk::CommandBuffer,
            signal_semaphore_count: signal_semaphores.len() as u32,
            p_signal_semaphores: signal_semaphores.as_ptr(),
        };

        let fence = match &self.used_fence {
            Some(fence) => fence.clone(),
            None => Arc::new(initialized.pool().get_fence(&self.device)?),
        };

        unsafe {
            vulkan_device.queue_submit(
                self.queue_family.get_queue().queue,
                &[submit_info],
                fence.inner(),
            )
        }?;

        // Presentation.
        if !self.attached_camera_windows.is_empty() {
            let mut swapchains = Vec::new();
            let mut image_indices = Vec::new();
            let mut results = Vec::new();

            for output in &self.attached_camera_windows {
                swapchains.push(output.pass.inner());
                image_indices.push(output.image_index);
                results.push(vk::Result::default());
            }

            let present_info = vk::PresentInfoKHR {
                s_type: vk::StructureType::PRESENT_INFO_KHR,
                p_next: ptr::null(),
                wait_semaphore_count: signal_semaphores.len() as u32,
                p_wait_semaphores: signal_semaphores.as_ptr(),
                swapchain_count: swapchains.len() as u32,
                p_swapchains: swapchains.as_ptr(),
                p_image_indices: image_indices.as_ptr(),
                p_results: results.as_mut_ptr(),
            };

            {
                let pass = &self.attached_camera_windows[0].pass;
                let present_queue = pass.present_family().get_queue();

                let _swapchain_lock = pass.lock_ash_swapchain()?;

                _ = unsafe {
                    pass.ash_swapchain()
                        .queue_present(present_queue.queue, &present_info)
                };
            }

            for output in &self.attached_camera_windows {
                output.synchronized_fence.set_reset_event();
            }

            let mut i = 0;
            for result in results {
                if result == vk::Result::SUCCESS {
                    i += 1;
                    continue;
                }

                if result != vk::Result::ERROR_OUT_OF_DATE_KHR
                    || result != vk::Result::SUBOPTIMAL_KHR
                {
                    if let Some(swapchain) =
                        Weak::upgrade(self.attached_camera_windows[i].pass.swapchain())
                    {
                        swapchain.recreate(None)?;
                    }
                } else {
                    let a: Result<(), vk::Result> = Err(result);
                    a?;
                }

                i += 1;
            }
        }

        Ok(fence)
    }

    pub fn get_or_create_used_fence(
        &mut self,
    ) -> Result<Arc<VulkanFence<'init>>, VulkanUniversalError> {
        match &self.used_fence {
            Some(fence) => Ok(fence.clone()),
            None => {
                let fence = Arc::new(self.initialized.pool().get_fence(&self.device)?);
                self.used_fence = Some(fence.clone());
                Ok(fence)
            }
        }
    }

    fn record(
        &mut self,
        mut data: SerializationReader,
        simultaneous_execute: bool,
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

        unsafe { vulkan_device.begin_command_buffer(self.inner, &begin_info) }?;

        while let Some(command) = data.read::<GraphicsCommandBufferCommand>() {
            match command {
                GraphicsCommandBufferCommand::CopyBuffer => {
                    memory_commands::copy_buffer(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::CopyBufferToTexture => {
                    memory_commands::copy_buffer_to_texture(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::CopyTextureToBuffer => {
                    memory_commands::copy_texture_to_buffer(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::Dispatch => {
                    compute_commands::dispatch(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::AttachCameraWindow => {
                    let used_fence = self.get_or_create_used_fence()?;
                    self.attached_camera_windows
                        .push(camera_commands::attach_camera_window(
                            &mut data,
                            self,
                            vulkan_device,
                            &used_fence,
                        )?)
                }
                GraphicsCommandBufferCommand::AttachCameraTexture => {
                    camera_commands::attach_camera_texture(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::DetachCamera => {
                    camera_commands::detach_camera(self, vulkan_device)
                }
                GraphicsCommandBufferCommand::DrawMesh => {
                    draw_commands::draw_mesh(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::AttachPipeline => {
                    self.attached_pipeline_layout =
                        misc_commands::attach_shader(&mut data, self, vulkan_device)
                }
                GraphicsCommandBufferCommand::AttachMaterial => {
                    misc_commands::attach_material(&mut data, self, vulkan_device)
                }
            };
        }

        unsafe { vulkan_device.end_command_buffer(self.inner) }?;

        Ok(())
    }
}

impl Drop for VulkanCommandBuffer<'_, '_> {
    fn drop(&mut self) {
        let initialized = self.initialized;

        // https://arm-software.github.io/vulkan_best_practice_for_mobile_developers/samples/performance/command_buffer_usage/command_buffer_usage_tutorial.html#allocate-and-free
        /*unsafe {
            initialized.vulkan_device().reset_command_pool(
                self.command_pool.inner(), vk::CommandPoolResetFlags::empty()
            )
        }.unwrap();*/

        unsafe {
            initialized
                .vulkan_device()
                .free_command_buffers(self.command_pool.inner(), &[self.inner])
        }
    }
}

impl<'init> GraphicsCommandBuffer<'init> for VulkanCommandBuffer<'init, '_> {
    fn execute(&self) -> InteropResult<Box<Arc<dyn GraphicsFence + 'init>>> {
        match self.execute() {
            Ok(fence) => InteropResult::with_ok(Box::new(fence)),
            Err(err) => InteropResult::with_err(err.into()),
        }
    }
}
