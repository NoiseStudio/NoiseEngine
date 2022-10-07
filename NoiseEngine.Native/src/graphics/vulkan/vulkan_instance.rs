use std::sync::Arc;

use vulkano::instance::{Instance, InstanceCreationError};

use crate::interop::graphics::vulkan::vulkan_instance_create_info::VulkanInstanceCreateInfo;

pub(crate) fn create(create_info: VulkanInstanceCreateInfo) -> Result<Arc<Instance>, InstanceCreationError> {
    Instance::new(create_info.into())
}
