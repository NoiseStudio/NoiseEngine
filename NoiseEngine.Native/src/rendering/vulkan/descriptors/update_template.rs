use std::{ptr, sync::Arc};

use ash::vk;

use crate::rendering::vulkan::errors::universal::VulkanUniversalError;

use super::set_layout::DescriptorSetLayout;

pub struct DescriptorUpdateTemplate<'init> {
    layout: Arc<DescriptorSetLayout<'init>>,
    inner: vk::DescriptorUpdateTemplate
}

impl<'init> DescriptorUpdateTemplate<'init> {
    pub fn new(
        layout: &Arc<DescriptorSetLayout<'init>>, entries: &[vk::DescriptorUpdateTemplateEntry]
    ) -> Result<Self, VulkanUniversalError> {
        let create_info = vk::DescriptorUpdateTemplateCreateInfo {
            s_type: vk::StructureType::DESCRIPTOR_UPDATE_TEMPLATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::DescriptorUpdateTemplateCreateFlags::empty(),
            descriptor_update_entry_count: entries.len() as u32,
            p_descriptor_update_entries: entries.as_ptr(),
            template_type: vk::DescriptorUpdateTemplateType::default(),
            descriptor_set_layout: layout.inner(),
            pipeline_bind_point: vk::PipelineBindPoint::default(),
            pipeline_layout: vk::PipelineLayout::default(),
            set: 0,
        };

        let initialized = layout.device().initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_descriptor_update_template(&create_info, None)
        }?;

        Ok(Self { layout: layout.clone(), inner })
    }

    pub fn inner(&self) -> vk::DescriptorUpdateTemplate {
        self.inner
    }
}

impl Drop for DescriptorUpdateTemplate<'_> {
    fn drop(&mut self) {
        unsafe {
            self.layout.device().initialized().unwrap().vulkan_device().destroy_descriptor_update_template(
                self.inner, None
            );
        }
    }
}
