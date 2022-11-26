use std::ptr;

use ash::vk;

use super::{
    errors::universal::VulkanUniversalError, pipeline_layout::PipelineLayout,
    pipeline_shader_stage::PipelineShaderStage, device::VulkanDeviceInitialized
};

pub struct Pipeline<'init> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::Pipeline
}

impl<'init, 'pipl: 'init> Pipeline<'init> {
    pub fn with_compute(
        layout: &'pipl PipelineLayout<'init>, stage: PipelineShaderStage, flags: vk::PipelineCreateFlags
    ) -> Result<Self, VulkanUniversalError> {
        let final_stage =
            Result::<vk::PipelineShaderStageCreateInfo, VulkanUniversalError>::from(stage)?;

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

        Ok(Self { initialized, inner })
    }

    pub fn inner(&self) -> vk::Pipeline {
        self.inner
    }
}

impl Drop for Pipeline<'_> {
    fn drop(&mut self) {
        unsafe {
            self.initialized.vulkan_device().destroy_pipeline(self.inner, None);
        }
    }
}
