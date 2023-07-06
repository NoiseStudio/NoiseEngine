use std::sync::Arc;

use ash::vk;

use crate::{
    interop::{interop_read_only_span::InteropReadOnlySpan, prelude::InteropResult},
    rendering::vulkan::descriptors::{
        set_layout::DescriptorSetLayout, update_template::DescriptorUpdateTemplate,
    },
};

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_update_template_create<'init>(
    layout: &Arc<DescriptorSetLayout<'init>>,
    entries: InteropReadOnlySpan<vk::DescriptorUpdateTemplateEntry>,
) -> InteropResult<Box<DescriptorUpdateTemplate<'init>>> {
    match DescriptorUpdateTemplate::new(layout, entries.into()) {
        Ok(d) => InteropResult::with_ok(Box::new(d)),
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_update_template_destroy(
    _handle: Box<DescriptorUpdateTemplate>,
) {
}
