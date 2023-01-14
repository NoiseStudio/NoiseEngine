use std::{ptr, sync::{Arc, Mutex, MutexGuard, Weak}, mem};

use arc_cell::{AtomicCell, ArcCell};
use ash::{vk, extensions::khr, prelude::VkResult};

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
    dynamic: ArcCell<SwapchainSharedDynamic<'init, 'fam>>,
    capabilities: vk::SurfaceCapabilitiesKHR,
    format: vk::SurfaceFormatKHR,
    present_mode: vk::PresentModeKHR,
    pass: AtomicCell<Option<Arc<SwapchainPass<'init, 'fam>>>>,
    mutex: Mutex<()>,
    device: Arc<VulkanDevice<'init>>
}

impl<'init: 'fam, 'fam> Swapchain<'init, 'fam> {
    pub fn new(
        device: &'init Arc<VulkanDevice<'init>>, surface: VulkanSurface
    ) -> Result<Arc<Self>, VulkanUniversalError> {
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
        let shared = Arc::new(SwapchainShared {
            swapchain: Weak::default(),
            surface,
            ash_swapchain: khr::Swapchain::new(instance.inner(), initialized.vulkan_device()),
            queue_family,
        });

        let swapchain = Arc::new(Self {
            shared: shared.clone(),
            dynamic: ArcCell::new(Arc::new(SwapchainSharedDynamic {
                shared: shared.clone(),
                inner: unsafe { mem::zeroed() },
                extent: vk::Extent2D::default(),
                image_views: Vec::with_capacity(0),
                image_available_semaphores: Vec::with_capacity(0),
                render_finished_semaphores: Vec::with_capacity(0),
            })),
            capabilities,
            format,
            present_mode,
            pass: AtomicCell::new(None),
            mutex: Mutex::new(()),
            device: device.clone()
        });

        let reference = unsafe {
            &mut *(Arc::as_ptr(&shared) as *mut SwapchainShared)
        };
        reference.swapchain = Arc::downgrade(&swapchain);

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

    pub fn extent(&self) -> vk::Extent2D {
        self.shared.surface.window().get_vulkan_extent()
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

        self.create_pass(render_pass)
    }

    pub fn get_swapchain_pass_and_accquire_next_image(
        &'init self, render_pass: &Arc<RenderPass<'init>>) -> Result<(Arc<SwapchainPass<'init, 'fam>>, u32
    ), VulkanUniversalError> {
        loop {
            let swapchain_pass = self.get_swapchain_pass(render_pass)?;
            match swapchain_pass.accquire_next_image() {
                Ok((i, b)) => {
                    if !b {
                        return Ok((swapchain_pass, i));
                    }
                },
                Err(result) => {
                    if result != vk::Result::ERROR_OUT_OF_DATE_KHR {
                        return Err(result.into())
                    }
                },
            }

            self.recreate()?;
        }
    }

    pub fn recreate(&self) -> Result<(), VulkanUniversalError> {
        // Lock mutex.
        let _lock = self.lock_mutex()?;

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
            image_extent: self.extent(),
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
            old_swapchain: self.dynamic.get().inner
        };

        let inner = unsafe {
            self.shared.ash_swapchain.create_swapchain(&create_info, None)
        }?;

        self.create_dynamic(inner, create_info.image_extent)?;
        self.pass.set(None);

        Ok(())
    }

    fn create_dynamic(&self, inner: vk::SwapchainKHR, extent: vk::Extent2D) -> Result<(), VulkanUniversalError> {
        // Images.
        let images = unsafe {
            self.shared.ash_swapchain.get_swapchain_images(inner)
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
            shared: self.shared.clone(),
            inner,
            extent,
            image_views,
            image_available_semaphores,
            render_finished_semaphores,
        }));

        Ok(())
    }

    fn create_pass(
        &'init self, render_pass: &Arc<RenderPass<'init>>
    ) -> Result<Arc<SwapchainPass<'init, 'fam>>, VulkanUniversalError> {
        // Lock mutex.
        let _lock = self.lock_mutex()?;

        // Create new pass.
        let dynamic = self.dynamic.get();

        let mut framebuffers = Vec::with_capacity(dynamic.image_views.len());
        for image_view in &dynamic.image_views {
            framebuffers.push(SwapchainFramebuffer::new(&render_pass, image_view, dynamic.extent)?)
        }

        let new_pass = Arc::new(SwapchainPass {
            shared: self.shared.clone(),
            dynamic,
            framebuffers,
            render_pass: render_pass.clone(),
        });

        self.pass.set(Some(new_pass.clone()));
        Ok(new_pass)
    }

    fn lock_mutex(&self) -> Result<MutexGuard<()>, InvalidOperationError> {
        match self.mutex.lock() {
            Ok(l) => Ok(l),
            Err(_) => Err(InvalidOperationError::with_str("Another thread holding the mutex panicked.")),
        }
    }
}

struct SwapchainShared<'init: 'fam, 'fam> {
    swapchain: Weak<Swapchain<'init, 'fam>>,
    surface: VulkanSurface,
    ash_swapchain: khr::Swapchain,
    queue_family: &'fam VulkanQueueFamily<'init>,
}

struct SwapchainSharedDynamic<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    inner: vk::SwapchainKHR,
    extent: vk::Extent2D,
    image_views: Vec<Arc<SwapchainImageView<'init>>>,
    image_available_semaphores: Vec<VulkanSemaphore<'init>>,
    render_finished_semaphores: Vec<VulkanSemaphore<'init>>
}

impl Drop for SwapchainSharedDynamic<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            self.shared.ash_swapchain.destroy_swapchain(self.inner, None)
        }
    }
}

pub struct SwapchainPass<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    dynamic: Arc<SwapchainSharedDynamic<'init, 'fam>>,
    framebuffers: Vec<SwapchainFramebuffer<'init>>,
    render_pass: Arc<RenderPass<'init>>
}

impl<'init: 'fam, 'fam> SwapchainPass<'init, 'fam> {
    pub fn accquire_next_image(&self) -> VkResult<(u32, bool)> {
        Ok(unsafe {
            self.shared.ash_swapchain.acquire_next_image(
                self.inner(), u64::MAX, self.get_image_available_semaphore().inner(),
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
        self.dynamic.inner
    }

    pub fn swapchain(&self) -> &Weak<Swapchain<'init, 'fam>> {
        &self.shared.swapchain
    }

    pub fn ash_swapchain(&self) -> &khr::Swapchain {
        &self.shared.ash_swapchain
    }

    pub fn present_family(&self) -> &'fam VulkanQueueFamily<'init> {
        self.shared.queue_family
    }
}
