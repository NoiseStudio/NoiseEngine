use std::sync::Arc;

use ash::vk;

use crate::{
    interop::{interop_read_only_span::InteropReadOnlySpan, prelude::InteropResult},
    rendering::vulkan::{
        graphics_pipeline_create_info::GraphicsPipelineCreateInfo, pipeline::Pipeline,
        pipeline_layout::PipelineLayout, pipeline_shader_stage::PipelineShaderStage,
        render_pass::RenderPass,
    },
};

#[no_mangle]
extern "C" fn rendering_vulkan_graphics_pipeline_create<'init>(
    render_pass: &Arc<RenderPass<'init>>,
    layout: &Arc<PipelineLayout<'init>>,
    stages: InteropReadOnlySpan<PipelineShaderStage>,
    flags: vk::PipelineCreateFlags,
    create_info: GraphicsPipelineCreateInfo,
) -> InteropResult<Box<Pipeline<'init>>> {
    match Pipeline::with_graphics(render_pass, layout, stages.into(), flags, create_info) {
        Ok(p) => InteropResult::with_ok(Box::new(p)),
        Err(err) => InteropResult::with_err(err.into()),
    }
}
