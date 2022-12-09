use ash::vk;

use crate::interop::prelude::InteropString;

use super::shader_module::ShaderModule;

#[repr(C)]
pub struct PipelineShaderStage<'init: 'shm, 'shm> {
    pub stage: vk::ShaderStageFlags,
    pub module: &'shm ShaderModule<'init>,
    pub name: InteropString
}
