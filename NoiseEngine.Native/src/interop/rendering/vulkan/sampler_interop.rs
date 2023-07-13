use std::sync::Arc;

use ash::vk;

use crate::{
    interop::prelude::InteropResult,
    rendering::{
        texture_sampler::{TextureSampler, TextureSamplerCreateInfo},
        vulkan::{device::VulkanDevice, sampler::VulkanSampler},
    },
};

#[repr(C)]
struct VulkanSamplerCreateReturnValue<'init> {
    pub handle: Box<Arc<dyn TextureSampler + 'init>>,
    pub inner_handle: vk::Sampler,
}

#[no_mangle]
extern "C" fn rendering_vulkan_sampler_interop_create<'init>(
    device: &'init Arc<VulkanDevice<'init>>,
    create_info: TextureSamplerCreateInfo,
) -> InteropResult<VulkanSamplerCreateReturnValue> {
    match VulkanSampler::new(device, create_info) {
        Ok(sampler) => {
            let inner = sampler.inner();
            InteropResult::with_ok(VulkanSamplerCreateReturnValue {
                handle: Box::new(Arc::new(sampler)),
                inner_handle: inner,
            })
        }
        Err(err) => InteropResult::with_err(err.into()),
    }
}
