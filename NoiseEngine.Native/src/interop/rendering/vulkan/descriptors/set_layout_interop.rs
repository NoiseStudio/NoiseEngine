use ash::vk;

use crate::{
    rendering::vulkan::{descriptors::set_layout::DescriptorSetLayout, device::VulkanDevice},
    interop::prelude::{InteropResult, InteropReadOnlySpan}
};

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_layout_create<'dev: 'init, 'init: 'setl, 'setl>(
    device: &'dev VulkanDevice<'_, 'init>, flags: vk::DescriptorSetLayoutCreateFlags,
    bindings: InteropReadOnlySpan<vk::DescriptorSetLayoutBinding>
) -> InteropResult<Box<DescriptorSetLayout<'init>>> {
    match DescriptorSetLayout::new(device, flags, bindings.into()) {
        Ok(d) => InteropResult::with_ok(Box::new(d)),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_descriptors_set_layout_destroy(_handle: Box<DescriptorSetLayout>) {
}
