use ash::vk;

use crate::interop::prelude::InteropString;

use super::shader_module::ShaderModule;

#[repr(C)]
pub struct PipelineShaderStage<'shm> {
    pub stage: vk::ShaderStageFlags,
    pub module: &'shm ShaderModule<'shm>,
    pub name: InteropString
}
