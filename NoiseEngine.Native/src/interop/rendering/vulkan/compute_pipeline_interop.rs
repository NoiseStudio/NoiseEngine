use std::sync::Arc;

use ash::vk;

use crate::{
    interop::prelude::InteropResult,
    rendering::vulkan::{
        pipeline::Pipeline, pipeline_layout::PipelineLayout,
        pipeline_shader_stage::PipelineShaderStage,
    },
};

#[no_mangle]
extern "C" fn rendering_vulkan_compute_pipeline_create<'init>(
    layout: &Arc<PipelineLayout<'init>>,
    stage: PipelineShaderStage,
    flags: vk::PipelineCreateFlags,
) -> InteropResult<Box<Pipeline<'init>>> {
    match Pipeline::with_compute(layout, stage, flags) {
        Ok(p) => InteropResult::with_ok(Box::new(p)),
        Err(err) => InteropResult::with_err(err.into()),
    }
}
