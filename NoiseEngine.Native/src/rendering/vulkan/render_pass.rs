use std::{sync::Arc, ptr};

use ash::vk;

use crate::rendering::camera_clear::CameraClearFlags;

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError};

#[repr(C)]
pub struct RenderPassCreateInfo {
    format: vk::Format,
    sample_count: vk::SampleCountFlags,
    clear_flags: CameraClearFlags
}

pub struct RenderPass<'init> {
    inner: vk::RenderPass,
    device: Arc<VulkanDevice<'init>>
}

impl<'init> RenderPass<'init> {
    pub fn new(
        device: &Arc<VulkanDevice<'init>>, create_info: RenderPassCreateInfo
    ) -> Result<Self, VulkanUniversalError> {
        let color_attachment = vk::AttachmentDescription {
            flags: vk::AttachmentDescriptionFlags::default(),
            format: create_info.format,
            samples: create_info.sample_count,
            load_op: match create_info.clear_flags {
                CameraClearFlags::Undefined => vk::AttachmentLoadOp::DONT_CARE,
                CameraClearFlags::Nothing => vk::AttachmentLoadOp::LOAD,
                CameraClearFlags::SolidColor => vk::AttachmentLoadOp::CLEAR,
            },
            store_op: vk::AttachmentStoreOp::STORE,
            stencil_load_op: vk::AttachmentLoadOp::DONT_CARE,
            stencil_store_op: vk::AttachmentStoreOp::DONT_CARE,
            initial_layout: vk::ImageLayout::UNDEFINED,
            final_layout: vk::ImageLayout::TRANSFER_DST_OPTIMAL,
        };

        let color_attachment_reference = vk::AttachmentReference {
            attachment: 0,
            layout: vk::ImageLayout::COLOR_ATTACHMENT_OPTIMAL,
        };

        let subpass = vk::SubpassDescription {
            flags: vk::SubpassDescriptionFlags::default(),
            pipeline_bind_point: vk::PipelineBindPoint::GRAPHICS,
            input_attachment_count: 0,
            p_input_attachments: ptr::null(),
            color_attachment_count: 1,
            p_color_attachments: &color_attachment_reference,
            p_resolve_attachments: ptr::null(),
            p_depth_stencil_attachment: ptr::null(),
            preserve_attachment_count: 0,
            p_preserve_attachments: ptr::null(),
        };

        let vk_create_info = vk::RenderPassCreateInfo {
            s_type: vk::StructureType::RENDER_PASS_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::RenderPassCreateFlags::default(),
            attachment_count: 1,
            p_attachments: &color_attachment,
            subpass_count: 1,
            p_subpasses: &subpass,
            dependency_count: 0,
            p_dependencies: ptr::null(),
        };

        let initialized = device.initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_render_pass(&vk_create_info, None)
        }?;

        Ok(Self { inner, device: device.clone() })
    }

    pub fn inner(&self) -> vk::RenderPass {
        self.inner
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }
}

impl Drop for RenderPass<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device.initialized().unwrap().vulkan_device().destroy_render_pass(
                self.inner, None
            );
        }
    }
}
