use std::sync::Arc;

use ash::vk;

use crate::{
    interop::prelude::{InteropReadOnlySpan, InteropResult},
    rendering::vulkan::{descriptors::set_layout::DescriptorSetLayout, device::VulkanDevice},
};

#[allow(clippy::redundant_allocation)]
#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_layout_create<'dev: 'init, 'init: 'setl, 'setl>(
    device: &'dev Arc<VulkanDevice<'init>>,
    flags: vk::DescriptorSetLayoutCreateFlags,
    bindings: InteropReadOnlySpan<vk::DescriptorSetLayoutBinding>,
) -> InteropResult<Box<Arc<DescriptorSetLayout<'init>>>> {
    match DescriptorSetLayout::new(device, flags, bindings.into()) {
        Ok(d) => InteropResult::with_ok(Box::new(Arc::new(d))),
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_layout_destroy(
    _handle: Box<Arc<DescriptorSetLayout>>,
) {
}
