use std::sync::Arc;

use ash::vk;

use crate::{
    interop::prelude::InteropResult,
    rendering::{
        vulkan::{
            image::VulkanImage, image_view::{VulkanImageView, VulkanImageViewCreateInfo},
        },
    },
};

#[repr(C)]
struct VulkanImageViewCreateValue<'init: 'ma, 'ma> {
    image: &'init Arc<VulkanImage<'init, 'ma>>,
    create_info: VulkanImageViewCreateInfo
}

#[repr(C)]
struct VulkanImageViewCreateReturnValue<'init: 'ma, 'ma> {
    pub handle: Box<VulkanImageView<'init, 'ma>>,
    pub inner_handle: vk::ImageView,
}

#[no_mangle]
extern "C" fn rendering_vulkan_image_view_interop_create<'init: 'ma, 'ma>(
    value: VulkanImageViewCreateValue<'init, 'ma>,
) -> InteropResult<VulkanImageViewCreateReturnValue<'init, 'ma>> {
    match VulkanImageView::new(value.image, &value.create_info) {
        Ok(view) => {
            let inner = view.inner();
            InteropResult::with_ok(VulkanImageViewCreateReturnValue {
                handle: Box::new(view),
                inner_handle: inner,
            })
        }
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_image_view_interop_destroy(_handle: Box<VulkanImageView<'_, '_>>) {}
