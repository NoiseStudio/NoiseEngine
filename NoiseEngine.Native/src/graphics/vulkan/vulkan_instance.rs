use std::sync::Arc;

use vulkano::{instance::{Instance, InstanceCreationError, debug::{DebugUtilsMessenger, DebugUtilsMessengerCreateInfo, DebugUtilsMessengerCreationError}}, VulkanLibrary};

use crate::interop::graphics::vulkan::vulkan_instance_create_info::VulkanInstanceCreateInfo;

pub(crate) fn create(
    library: Arc<VulkanLibrary>, create_info: VulkanInstanceCreateInfo
) -> Result<Arc<Instance>, InstanceCreationError> {
    Instance::new(library, create_info.into())
}

/*pub(crate) fn register_debug_callback(instance: &Instance) -> Result<DebugUtilsMessenger, DebugUtilsMessengerCreationError> {
    DebugUtilsMessenger::new(
        instance,
        DebugUtilsMessengerCreateInfo::user_callback(Arc::new(|msg| {

        }))
    )
}*/
