use std::sync::Arc;

use crate::rendering::vulkan::{
    surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError
};

pub trait Window {
    fn create_vulkan_surface(&self, instance: &Arc<VulkanInstance>) -> Result<VulkanSurface, VulkanUniversalError>;
}
