use std::{ptr, sync::Arc};

use ash::vk;

use crate::rendering::vulkan::errors::universal::VulkanUniversalError;

use super::{descriptors::set_layout::DescriptorSetLayout, device::VulkanDevice};

pub struct PipelineLayout<'init> {
    inner: vk::PipelineLayout,
    device: Arc<VulkanDevice<'init>>,
}

impl<'init: 'setl, 'setl> PipelineLayout<'init> {
    pub fn new(
        layouts: &[&'setl Arc<DescriptorSetLayout<'init>>],
        push_constant_ranges: &[vk::PushConstantRange],
    ) -> Result<Self, VulkanUniversalError> {
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
            push_constant_range_count: push_constant_ranges.len() as u32,
            p_push_constant_ranges: push_constant_ranges.as_ptr(),
        };

        let device = layouts[0].device();
        let initialized = device.initialized()?;
        let inner = unsafe {
            initialized
                .vulkan_device()
                .create_pipeline_layout(&create_info, None)
        }?;

        Ok(Self {
            inner,
            device: device.clone(),
        })
    }

    pub fn inner(&self) -> vk::PipelineLayout {
        self.inner
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }
}

impl Drop for PipelineLayout<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_pipeline_layout(self.inner, None);
        }
    }
}
