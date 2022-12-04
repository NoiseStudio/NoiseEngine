use crate::{
    rendering::vulkan::{pipeline_layout::PipelineLayout, descriptors::set_layout::DescriptorSetLayout},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan}
};

#[no_mangle]
extern "C" fn rendering_vulkan_pipeline_layout_create<'init, 'setl: 'init>(
    layouts: InteropReadOnlySpan<&'setl DescriptorSetLayout<'init>>
) -> InteropResult<Box<PipelineLayout<'init>>> {
    match PipelineLayout::new(layouts.into()) {
        Ok(p) => InteropResult::with_ok(Box::new(p)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_pipeline_layout_destroy(_handle: Box<PipelineLayout>) {
}
