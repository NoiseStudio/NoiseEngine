use std::sync::Arc;

use ash::vk;

use crate::rendering::vulkan::{
    surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError
};

pub trait Window {
    fn get_width(&self) -> u32;
    fn get_height(&self) -> u32;

    fn pool_events(&self);
    fn hide(&self);

    fn create_vulkan_surface(&self, instance: &Arc<VulkanInstance>) -> Result<VulkanSurface, VulkanUniversalError>;

    fn get_vulkan_extent(&self) -> vk::Extent2D {
        vk::Extent2D {
            width: self.get_width(),
            height: self.get_height()
        }
    }
}
