use std::ptr;

use ash::vk;

use crate::rendering::vulkan::errors::universal::VulkanUniversalError;

use super::{descriptors::set_layout::DescriptorSetLayout, device::VulkanDeviceInitialized};

pub struct PipelineLayout<'init> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::PipelineLayout
}

impl<'init, 'setl: 'init> PipelineLayout<'init> {
    pub fn new(layouts: &[&'setl DescriptorSetLayout<'init>]) -> Result<Self, VulkanUniversalError> {
        let mut final_layouts = Vec::with_capacity(layouts.len());
        for layout in layouts {
            final_layouts.push(layout.inner());
        }

        let create_info = vk::PipelineLayoutCreateInfo {
            s_type: vk::StructureType::PIPELINE_LAYOUT_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineLayoutCreateFlags::empty(),
            set_layout_count: final_layouts.len() as u32,
            p_set_layouts: final_layouts.as_ptr(),
            push_constant_range_count: 0,
            p_push_constant_ranges: ptr::null(),
        };

        let initialized = layouts[0].initialized();
        let inner = unsafe {
            initialized.vulkan_device().create_pipeline_layout(&create_info, None)
        }?;

        Ok(Self { initialized, inner })
    }

    pub fn inner(&self) -> vk::PipelineLayout {
        self.inner
    }

    pub(crate) fn initialized(&self) ->  &'init VulkanDeviceInitialized<'init> {
        self.initialized
    }
}

impl Drop for PipelineLayout<'_> {
    fn drop(&mut self) {
        unsafe {
            self.initialized.vulkan_device().destroy_pipeline_layout(self.inner, None);
        }
    }
}
