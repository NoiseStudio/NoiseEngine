use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::{
        pipeline_shader_stage::PipelineShaderStage, pipeline_layout::PipelineLayout, pipeline::Pipeline
    },
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn rendering_vulkan_compute_pipeline_create<'init>(
    layout: &Arc<PipelineLayout<'init>>, stage: PipelineShaderStage, flags: vk::PipelineCreateFlags
) -> InteropResult<Box<Pipeline<'init>>> {
    match Pipeline::with_compute(layout, stage, flags) {
        Ok(p) => InteropResult::with_ok(Box::new(p)),
        Err(err) => InteropResult::with_err(err.into())
    }
}
