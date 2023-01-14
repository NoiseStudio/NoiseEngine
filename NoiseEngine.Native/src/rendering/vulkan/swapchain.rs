use std::{ptr, sync::{Arc, Mutex}, mem};

use arc_cell::{AtomicCell, ArcCell};
use ash::{vk, extensions::khr};

use crate::{
    rendering::errors::window_not_supported::WindowNotSupportedError, errors::invalid_operation::InvalidOperationError
};

use super::{
    surface::VulkanSurface, device::{VulkanDevice, VulkanQueueFamily}, errors::universal::VulkanUniversalError,
    render_pass::RenderPass, swapchain_image_view::SwapchainImageView, swapchain_framebuffer::SwapchainFramebuffer,
    semaphore::VulkanSemaphore
};

pub struct Swapchain<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    dynamic: ArcCell<SwapchainSharedDynamic<'init>>,
    capabilities: vk::SurfaceCapabilitiesKHR,
    format: vk::SurfaceFormatKHR,
    present_mode: vk::PresentModeKHR,
    pass: AtomicCell<Option<Arc<SwapchainPass<'init, 'fam>>>>,
    pass_mutex: Mutex<()>,
    inner: vk::SwapchainKHR,
    device: Arc<VulkanDevice<'init>>
}

impl<'init: 'fam, 'fam> Swapchain<'init, 'fam> {
    pub fn new(device: &'init Arc<VulkanDevice<'init>>, surface: VulkanSurface) -> Result<Self, VulkanUniversalError> {
        let instance = device.instance();
        let initialized = device.initialized()?;
        let ash_surface = khr::Surface::new(instance.library(), instance.inner());

        // Capabilities.
        let capabilities = unsafe {
            ash_surface.get_physical_device_surface_capabilities(device.physical_device(), surface.inner())
        }?;

        // Format.
        let available_formats = unsafe {
            ash_surface.get_physical_device_surface_formats(device.physical_device(), surface.inner())
        }?;

        if available_formats.is_empty() {
            return Err(WindowNotSupportedError::with_entry_str(
                "Graphics device and surface does not have compatible formats."
            ).into())
        }

        let format = Self::choose_surface_format(available_formats);

        // Present modes.
        let available_present_modes = unsafe {
            ash_surface.get_physical_device_surface_present_modes(device.physical_device(), surface.inner())
        }?;

        if available_present_modes.is_empty() {
            return Err(WindowNotSupportedError::with_entry_str(
                "Graphics device and surface does not have compatible present modes."
            ).into())
        }

        let present_mode = Self::choose_surface_present_mode(available_present_modes);

        // Queue family.
        let mut queue_family_option = None;

        // Reverse the order of the families to find a matching family with the least feature coverage.
        // Example: https://vulkan.gpuinfo.org/displayreport.php?id=18393#queuefamilies
        //          This ensures that the compute family is used for this example GPU.
        for family in initialized.get_families().iter().rev() {
            let supports = unsafe {
                ash_surface.get_physical_device_surface_support(
                    device.physical_device(), family.index(), surface.inner()
                )
            }?;

            if supports {
                queue_family_option = Some(family);
                break;
            }
        }

        let queue_family = match queue_family_option {
            Some(q) => q,
            None => return Err(WindowNotSupportedError::with_entry_str(
                "Graphics device and surface does not have compatible present queue families."
            ).into()),
        };

        // Construct.
        let mut swapchain = Self {
            shared: Arc::new(SwapchainShared {
                surface,
                ash_swapchain: khr::Swapchain::new(instance.inner(), initialized.vulkan_device()),
                queue_family,
            }),
            dynamic: ArcCell::new(Arc::new(SwapchainSharedDynamic {
                image_views: Vec::with_capacity(0),
                image_available_semaphores: Vec::with_capacity(0),
                render_finished_semaphores: Vec::with_capacity(0),
            })),
            capabilities,
            format,
            present_mode,
            pass: AtomicCell::new(None),
            pass_mutex: Mutex::new(()),
            inner: unsafe { mem::zeroed() },
            device: device.clone()
        };
        swapchain.recreate()?;

        Ok(swapchain)
    }

    fn choose_surface_format(available_formats: Vec<vk::SurfaceFormatKHR>) -> vk::SurfaceFormatKHR {
        for format in &available_formats {
            if format.format == vk::Format::B8G8R8A8_SRGB && format.color_space == vk::ColorSpaceKHR::SRGB_NONLINEAR {
                return *format;
            }
        }

        available_formats[0]
    }

    fn choose_surface_present_mode(available_present_modes: Vec<vk::PresentModeKHR>) -> vk::PresentModeKHR {
        for available_present_mode in &available_present_modes {
            if available_present_mode.eq(&vk::PresentModeKHR::MAILBOX) {
                return *available_present_mode;
            }
        }

        available_present_modes[0]
    }

    pub fn format(&self) -> vk::SurfaceFormatKHR {
        self.format
    }

    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }

    pub fn get_swapchain_pass(
        &'init self, render_pass: &Arc<RenderPass<'init>>
    ) -> Result<Arc<SwapchainPass<'init, 'fam>>, VulkanUniversalError> {
        // Check if pass is created.
        match self.pass.get() {
            Some(current_pass) => {
                if Arc::ptr_eq(&current_pass.render_pass, render_pass) {
                    return Ok(current_pass)
                }
            },
            None => (),
        }

        // Lock pass mutex.
        let _lock = match self.pass_mutex.lock() {
            Ok(l) => l,
            Err(_) => return Err(
                InvalidOperationError::with_str("Another thread holding the mutex panicked.").into()
            ),
        };

        // Create new pass.
        let dynamic = self.dynamic.get();

        let mut framebuffers = Vec::with_capacity(dynamic.image_views.len());
        for image_view in &dynamic.image_views {
            framebuffers.push(SwapchainFramebuffer::new(&render_pass, image_view)?)
        }

        let new_pass = Arc::new(SwapchainPass {
            shared: self.shared.clone(),
            dynamic,
            inner: self.inner,
            framebuffers,
            render_pass: render_pass.clone(),
        });

        self.pass.set(Some(new_pass.clone()));
        Ok(new_pass)
    }

    fn recreate(&mut self) -> Result<(), VulkanUniversalError> {
        let initialized = self.device.initialized()?;

        let mut family_indices = Vec::new();
        for family in initialized.get_families() {
            family_indices.push(family.index());
        }

        let create_info = vk::SwapchainCreateInfoKHR {
            s_type: vk::StructureType::SWAPCHAIN_CREATE_INFO_KHR,
            p_next: ptr::null(),
            flags: vk::SwapchainCreateFlagsKHR::empty(),
            surface: self.shared.surface.inner(),
            min_image_count: self.capabilities.min_image_count,
            image_format: self.format.format,
            image_color_space: self.format.color_space,
            image_extent: vk::Extent2D { width: 1264, height: 681 },
            image_array_layers: 1,
            image_usage: vk::ImageUsageFlags::COLOR_ATTACHMENT,
            image_sharing_mode: match family_indices.is_empty() {
                true => vk::SharingMode::CONCURRENT,
                false => vk::SharingMode::EXCLUSIVE,
            },
            queue_family_index_count: family_indices.len() as u32,
            p_queue_family_indices: family_indices.as_ptr(),
            pre_transform: self.capabilities.current_transform,
            composite_alpha: vk::CompositeAlphaFlagsKHR::OPAQUE,
            present_mode: self.present_mode,
            clipped: 1,
            old_swapchain: self.inner
        };

        self.inner = unsafe {
            self.shared.ash_swapchain.create_swapchain(&create_info, None)
        }?;

        self.create_dynamic()?;

        Ok(())
    }

    fn create_dynamic(&self) -> Result<(), VulkanUniversalError> {
        // Images.
        let images = unsafe {
            self.shared.ash_swapchain.get_swapchain_images(self.inner)
        }?;

        let mut image_views = Vec::with_capacity(images.len());
        for image in images {
            image_views.push(Arc::new(SwapchainImageView::new(self, image)?));
        }

        // Semaphores.
        let initialized = self.device().initialized()?;

        let mut image_available_semaphores = Vec::with_capacity(image_views.len());
        let mut render_finished_semaphores = Vec::with_capacity(image_views.len());

        for _ in 0..image_views.len() {
            image_available_semaphores.push(initialized.pool().get_semaphore(&self.device)?);
            render_finished_semaphores.push(initialized.pool().get_semaphore(&self.device)?);
        }

        // Construct.
        self.dynamic.set(Arc::new(SwapchainSharedDynamic {
            image_views,
            image_available_semaphores,
            render_finished_semaphores,
        }));

        Ok(())
    }
}

impl Drop for Swapchain<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.shared.ash_swapchain.destroy_swapchain(self.inner, None)
        }
    }
}

struct SwapchainShared<'init: 'fam, 'fam> {
    surface: VulkanSurface,
    ash_swapchain: khr::Swapchain,
    queue_family: &'fam VulkanQueueFamily<'init>
}

struct SwapchainSharedDynamic<'init> {
    image_views: Vec<Arc<SwapchainImageView<'init>>>,
    image_available_semaphores: Vec<VulkanSemaphore<'init>>,
    render_finished_semaphores: Vec<VulkanSemaphore<'init>>
}

pub struct SwapchainPass<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    dynamic: Arc<SwapchainSharedDynamic<'init>>,
    inner: vk::SwapchainKHR,
    framebuffers: Vec<SwapchainFramebuffer<'init>>,
    render_pass: Arc<RenderPass<'init>>
}

impl<'init: 'fam, 'fam> SwapchainPass<'init, 'fam> {
    pub fn accquire_next_image(&self) -> Result<(u32, bool), VulkanUniversalError> {
        Ok(unsafe {
            self.shared.ash_swapchain.acquire_next_image(
                self.inner, u64::MAX, self.get_image_available_semaphore().inner(),
                vk::Fence::null()
            )
        }?)
    }

    pub fn get_framebuffer(&self, index: u32) -> &SwapchainFramebuffer<'init> {
        &self.framebuffers[index as usize]
    }

    pub fn get_image_available_semaphore(&self) -> &VulkanSemaphore<'init> {
        &self.dynamic.image_available_semaphores[0]
    }

    pub fn get_render_finished_semaphore(&self) -> &VulkanSemaphore<'init> {
        &self.dynamic.render_finished_semaphores[0]
    }

    pub fn inner(&self) -> vk::SwapchainKHR {
        self.inner
    }

    pub fn ash_swapchain(&self) -> &khr::Swapchain {
        &self.shared.ash_swapchain
    }

    pub fn present_family(&self) -> &'fam VulkanQueueFamily<'init> {
        self.shared.queue_family
    }
}
