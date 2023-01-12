use std::sync::Arc;

use ash::{vk, extensions::khr};

use crate::rendering::presentation::window::Window;

use super::instance::VulkanInstance;

pub struct VulkanSurface {
    instance: Arc<VulkanInstance>,
    _window: Arc<dyn Window>,
    inner: vk::SurfaceKHR
}

impl VulkanSurface {
    pub fn new(instance: Arc<VulkanInstance>, window: Arc<dyn Window>, inner: vk::SurfaceKHR) -> Self {
        Self { instance, _window: window, inner }
    }
}

impl Drop for VulkanSurface {
    fn drop(&mut self) {
        let surface = khr::Surface::new(self.instance.library(), self.instance.inner());
        unsafe {
            surface.destroy_surface(self.inner, None);
        }
    }
}
