use crate::rendering::vulkan::pipeline::Pipeline;

#[no_mangle]
extern "C" fn rendering_vulkan_pipeline_destroy(_handle: Box<Pipeline>) {}
