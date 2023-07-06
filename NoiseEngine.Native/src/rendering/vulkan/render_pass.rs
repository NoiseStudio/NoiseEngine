use std::{ptr, sync::Arc};

use ash::vk;

use crate::rendering::camera_clear::CameraClearFlags;

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError};

#[repr(C)]
pub struct RenderPassCreateInfo {
    format: vk::Format,
    sample_count: vk::SampleCountFlags,
    clear_flags: CameraClearFlags,
    final_layout: vk::ImageLayout,
    depth_testing: bool,
    depth_stencil_format: vk::Format,
    depth_stencil_sample_count: vk::SampleCountFlags,
}

pub struct RenderPass<'init> {
    inner: vk::RenderPass,
    device: Arc<VulkanDevice<'init>>,
    depth_testing: bool,
    depth_stencil_format: vk::Format,
    depth_stencil_sample_count: vk::SampleCountFlags,
}

impl<'init> RenderPass<'init> {
    pub fn new(
        device: &Arc<VulkanDevice<'init>>,
        create_info: RenderPassCreateInfo,
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
            final_layout: create_info.final_layout,
        };

        let color_attachment_reference = vk::AttachmentReference {
            attachment: 0,
            layout: vk::ImageLayout::COLOR_ATTACHMENT_OPTIMAL,
        };

        let depth_attachment;
        let depth_attachment_reference;

        let dependency;

        if create_info.depth_testing {
            depth_attachment = vk::AttachmentDescription {
                flags: vk::AttachmentDescriptionFlags::default(),
                format: create_info.depth_stencil_format,
                samples: create_info.depth_stencil_sample_count,
                load_op: vk::AttachmentLoadOp::CLEAR,
                store_op: vk::AttachmentStoreOp::DONT_CARE,
                stencil_load_op: vk::AttachmentLoadOp::DONT_CARE,
                stencil_store_op: vk::AttachmentStoreOp::DONT_CARE,
                initial_layout: vk::ImageLayout::UNDEFINED,
                final_layout: vk::ImageLayout::DEPTH_STENCIL_ATTACHMENT_OPTIMAL,
            };

            depth_attachment_reference = vk::AttachmentReference {
                attachment: 1,
                layout: vk::ImageLayout::DEPTH_STENCIL_ATTACHMENT_OPTIMAL,
            };

            dependency = vk::SubpassDependency {
                src_subpass: vk::SUBPASS_EXTERNAL,
                dst_subpass: 0,
                src_stage_mask: vk::PipelineStageFlags::COLOR_ATTACHMENT_OUTPUT
                    | vk::PipelineStageFlags::EARLY_FRAGMENT_TESTS,
                dst_stage_mask: vk::PipelineStageFlags::COLOR_ATTACHMENT_OUTPUT
                    | vk::PipelineStageFlags::EARLY_FRAGMENT_TESTS,
                src_access_mask: vk::AccessFlags::empty(),
                dst_access_mask: vk::AccessFlags::COLOR_ATTACHMENT_WRITE
                    | vk::AccessFlags::DEPTH_STENCIL_ATTACHMENT_WRITE,
                dependency_flags: vk::DependencyFlags::empty(),
            };
        } else {
            depth_attachment = vk::AttachmentDescription::default();
            depth_attachment_reference = vk::AttachmentReference::default();

            dependency = vk::SubpassDependency {
                src_subpass: vk::SUBPASS_EXTERNAL,
                dst_subpass: 0,
                src_stage_mask: vk::PipelineStageFlags::COLOR_ATTACHMENT_OUTPUT,
                dst_stage_mask: vk::PipelineStageFlags::COLOR_ATTACHMENT_OUTPUT,
                src_access_mask: vk::AccessFlags::empty(),
                dst_access_mask: vk::AccessFlags::COLOR_ATTACHMENT_WRITE,
                dependency_flags: vk::DependencyFlags::empty(),
            };
        }

        let subpass = vk::SubpassDescription {
            flags: vk::SubpassDescriptionFlags::default(),
            pipeline_bind_point: vk::PipelineBindPoint::GRAPHICS,
            input_attachment_count: 0,
            p_input_attachments: ptr::null(),
            color_attachment_count: 1,
            p_color_attachments: &color_attachment_reference,
            p_resolve_attachments: ptr::null(),
            p_depth_stencil_attachment: match create_info.depth_testing {
                true => &depth_attachment_reference,
                false => ptr::null(),
            },
            preserve_attachment_count: 0,
            p_preserve_attachments: ptr::null(),
        };

        let p_attachments = [color_attachment, depth_attachment];
        let vk_create_info = vk::RenderPassCreateInfo {
            s_type: vk::StructureType::RENDER_PASS_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::RenderPassCreateFlags::default(),
            attachment_count: match create_info.depth_testing {
                true => 2,
                false => 1,
            },
            p_attachments: p_attachments.as_ptr(),
            subpass_count: 1,
            p_subpasses: &subpass,
            dependency_count: 1,
            p_dependencies: &dependency,
        };

        let initialized = device.initialized()?;
        let inner = unsafe {
            initialized
                .vulkan_device()
                .create_render_pass(&vk_create_info, None)
        }?;

        Ok(Self {
            inner,
            device: device.clone(),
            depth_testing: create_info.depth_testing,
            depth_stencil_format: create_info.depth_stencil_format,
            depth_stencil_sample_count: create_info.depth_stencil_sample_count,
        })
    }

    pub fn inner(&self) -> vk::RenderPass {
        self.inner
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }

    pub fn depth_testing(&self) -> bool {
        self.depth_testing
    }

    pub fn depth_stencil_format(&self) -> vk::Format {
        self.depth_stencil_format
    }

    pub fn depth_stencil_sample_count(&self) -> vk::SampleCountFlags {
        self.depth_stencil_sample_count
    }
}

impl Drop for RenderPass<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_render_pass(self.inner, None);
        }
    }
}
