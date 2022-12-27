use std::sync::Arc;

use crate::{
    rendering::vulkan::descriptors::{set_layout::DescriptorSetLayout, set::DescriptorSet, update_template::DescriptorUpdateTemplate},
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan}
};

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_create<'set>(
    layout: &'set Arc<DescriptorSetLayout<'set>>
) -> InteropResult<Box<DescriptorSet<'set>>> {
    match DescriptorSet::new(layout) {
        Ok(d) => InteropResult::with_ok(Box::new(d)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_destroy(_handle: Box<DescriptorSet>) {
}

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_update(
    descriptor_set: &DescriptorSet, template: &DescriptorUpdateTemplate, data: InteropReadOnlySpan<u8>
) {
    descriptor_set.update(template, data.into());
}
