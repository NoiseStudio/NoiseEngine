use std::{ptr, ffi::CString};

use ash::vk;

use crate::errors::invalid_operation::InvalidOperationError;

use super::{
    errors::universal::VulkanUniversalError, pipeline_layout::PipelineLayout,
    pipeline_shader_stage::PipelineShaderStage
};

pub struct Pipeline<'init, 'pipl: 'init> {
    inner: vk::Pipeline,
    layout: &'pipl PipelineLayout<'init>
}

impl<'init, 'pipl: 'init> Pipeline<'init, 'pipl> {
    pub fn with_compute(
        layout: &'pipl PipelineLayout<'init>, stage: PipelineShaderStage, flags: vk::PipelineCreateFlags
    ) -> Result<Self, VulkanUniversalError> {
        let stage_name = match CString::new(String::from(stage.name)) {
            Ok(s) => s,
            Err(_) => return Err(
                InvalidOperationError::with_str("Pipeline shader stage name contains null character.").into()
            )
        };

        let final_stage = vk::PipelineShaderStageCreateInfo {
            s_type: vk::StructureType::PIPELINE_SHADER_STAGE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineShaderStageCreateFlags::empty(),
            stage: stage.stage,
            module: stage.module.inner(),
            p_name: stage_name.as_ptr(),
            p_specialization_info: ptr::null(),
        };

        let create_info = vk::ComputePipelineCreateInfo {
            s_type: vk::StructureType::COMPUTE_PIPELINE_CREATE_INFO,
            p_next: ptr::null(),
            flags,
            stage: final_stage,
            layout: layout.inner(),
            base_pipeline_handle: vk::Pipeline::null(),
            base_pipeline_index: 0,
        };

        let initialized = layout.initialized();
        let inner = match unsafe {
            initialized.vulkan_device().create_compute_pipelines(
                vk::PipelineCache::null(), &[create_info], None
            )
        } {
            Ok(i) => i[0],
            Err((_, err)) => return Err(err.into())
        };

        Ok(Self { inner, layout })
    }

    pub fn inner(&self) -> vk::Pipeline {
        self.inner
    }

    pub fn layout(&'pipl self) -> &PipelineLayout {
        &self.layout
    }
}

impl Drop for Pipeline<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.layout.initialized().vulkan_device().destroy_pipeline(self.inner, None);
        }
    }
}
