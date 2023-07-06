use std::sync::Arc;

use ash::{extensions::khr, vk};

use crate::rendering::presentation::window::Window;

use super::instance::VulkanInstance;

pub struct VulkanSurface {
    instance: Arc<VulkanInstance>,
    window: Arc<dyn Window>,
    inner: vk::SurfaceKHR,
    ash_surface: khr::Surface,
}

impl VulkanSurface {
    pub fn new(
        instance: Arc<VulkanInstance>,
        window: Arc<dyn Window>,
        inner: vk::SurfaceKHR,
        ash_surface: khr::Surface,
    ) -> Self {
        Self {
            instance,
            window,
            inner,
            ash_surface,
        }
    }

    pub fn inner(&self) -> vk::SurfaceKHR {
        self.inner
    }

    pub fn instance(&self) -> &Arc<VulkanInstance> {
        &self.instance
    }

    pub fn window(&self) -> &Arc<dyn Window> {
        &self.window
    }

    pub fn ash_surface(&self) -> &khr::Surface {
        &self.ash_surface
    }
}

impl Drop for VulkanSurface {
    fn drop(&mut self) {
        unsafe {
            self.ash_surface.destroy_surface(self.inner, None);
        }
    }
}
