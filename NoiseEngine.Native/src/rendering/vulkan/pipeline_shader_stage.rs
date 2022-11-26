use std::{ptr, ffi::CString};

use ash::vk;

use crate::{interop::prelude::InteropString, errors::invalid_operation::InvalidOperationError};

use super::{shader_module::ShaderModule, errors::universal::VulkanUniversalError};

#[repr(C)]
pub struct PipelineShaderStage<'shm> {
    pub stage: vk::ShaderStageFlags,
    pub module: &'shm ShaderModule<'shm>,
    pub name: InteropString
}

impl From<PipelineShaderStage<'_>> for Result<vk::PipelineShaderStageCreateInfo, VulkanUniversalError> {
    fn from(stage: PipelineShaderStage<'_>) -> Self {
        let name = match CString::new(String::from(stage.name)) {
            Ok(s) => s,
            Err(_) => return Err(
                InvalidOperationError::with_str("Pipeline shader stage name contains null character.").into()
            )
        };

        Ok(vk::PipelineShaderStageCreateInfo {
            s_type: vk::StructureType::PIPELINE_SHADER_STAGE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineShaderStageCreateFlags::empty(),
            stage: stage.stage,
            module: stage.module.inner(),
            p_name: name.as_ptr(),
            p_specialization_info: ptr::null(),
        })
    }
}
