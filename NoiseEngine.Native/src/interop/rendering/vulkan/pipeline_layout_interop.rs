use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::vulkan::{pipeline_layout::PipelineLayout, descriptors::set_layout::DescriptorSetLayout},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan}
};

#[allow(clippy::redundant_allocation)]
#[no_mangle]
extern "C" fn rendering_vulkan_pipeline_layout_create<'init: 'setl, 'setl>(
    layouts: InteropReadOnlySpan<&'setl Arc<DescriptorSetLayout<'init>>>,
    push_constant_ranges: InteropReadOnlySpan<vk::PushConstantRange>
) -> InteropResult<Box<Arc<PipelineLayout<'init>>>> {
    match PipelineLayout::new(layouts.into(), push_constant_ranges.into()) {
        Ok(p) => InteropResult::with_ok(Box::new(Arc::new(p))),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_pipeline_layout_destroy(_handle: Box<Arc<PipelineLayout>>) {
}
