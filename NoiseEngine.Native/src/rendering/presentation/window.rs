use std::sync::Arc;

use ash::vk;
use cgmath::Vector2;

use crate::{
    errors::platform::platform_universal::PlatformUniversalError,
    rendering::vulkan::{
        errors::universal::VulkanUniversalError, instance::VulkanInstance, surface::VulkanSurface,
    },
};

use super::input::InputData;

pub trait Window {
    fn get_width(&self) -> u32;
    fn get_height(&self) -> u32;

    fn poll_events(&self, input_data: &'static mut InputData);
    fn hide(&self);
    fn set_position(
        &self,
        position: Option<Vector2<i32>>,
        size: Option<Vector2<u32>>,
    ) -> Result<(), PlatformUniversalError>;
    fn set_cursor_position(&self, position: Vector2<f64>) -> Result<(), PlatformUniversalError>;
    fn set_title(&self, title: String) -> Result<(), PlatformUniversalError>;
    fn is_focused(&self) -> bool;
    fn dispose(&self) -> Result<(), PlatformUniversalError>;

    fn create_vulkan_surface(
        &self,
        instance: &Arc<VulkanInstance>,
    ) -> Result<VulkanSurface, VulkanUniversalError>;
    fn get_vulkan_extent(&self) -> vk::Extent2D {
        vk::Extent2D {
            width: self.get_width(),
            height: self.get_height(),
        }
    }
}
