use std::sync::Arc;

use ash::vk;

use crate::{
    rendering::{vulkan::{image::{VulkanImage, VulkanImageCreateInfo}, device::VulkanDevice}, texture::Texture},
    interop::prelude::InteropResult
};

#[repr(C)]
struct VulkanImageCreateReturnValue<'init> {
    pub handle: Box<Arc<dyn Texture + 'init>>,
    pub inner_handle: vk::Image
}

#[no_mangle]
extern "C" fn rendering_vulkan_image_interop_create<'init>(
    device: &'init Arc<VulkanDevice<'init>>, create_info: VulkanImageCreateInfo
) -> InteropResult<VulkanImageCreateReturnValue> {
    match VulkanImage::new(device, create_info) {
        Ok(image) => {
            let inner = image.inner();
            InteropResult::with_ok(VulkanImageCreateReturnValue {
                handle: Box::new(Arc::new(image)),
                inner_handle: inner
            })
        },
        Err(err) => InteropResult::with_err(err.into())
    }
}
