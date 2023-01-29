use std::{
    ptr, sync::{Arc, Mutex, MutexGuard, Weak, atomic::{AtomicUsize, Ordering}}, mem::{self, ManuallyDrop}, cmp,
    cell::Cell, rc::Rc, ops::Deref
};

use arc_cell::{AtomicCell, ArcCell, WeakCell};
use ash::{vk, extensions::khr};

use crate::{
    rendering::{errors::window_not_supported::WindowNotSupportedError}, errors::invalid_operation::InvalidOperationError
};

use super::{
    surface::VulkanSurface, device::{VulkanDevice, VulkanQueueFamily},
    errors::{universal::VulkanUniversalError, swapchain_accquire_next_image::SwapchainAccquireNextImageError},
    render_pass::RenderPass, swapchain_image_view::SwapchainImageView,
    swapchain_framebuffer::{SwapchainFramebuffer, SwapchainFramebufferAttachment},
    semaphore::VulkanSemaphore, swapchain_support::SwapchainSupport, synchronized_fence::VulkanSynchronizedFence,
    fence::VulkanFence, image::{VulkanImage, VulkanImageCreateInfo}, image_view::VulkanImageViewCreateInfo
};

pub struct Swapchain<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    dynamic: ArcCell<SwapchainSharedDynamic<'init, 'fam>>,
    format: vk::SurfaceFormatKHR,
    present_mode: vk::PresentModeKHR,
    pass: AtomicCell<Option<Arc<SwapchainPass<'init, 'fam>>>>,
    pass_creation_mutex: Mutex<()>,
    device: Arc<VulkanDevice<'init>>
}

impl<'init: 'fam, 'fam> Swapchain<'init, 'fam> {
    pub fn new(
        device: &'init Arc<VulkanDevice<'init>>, surface: VulkanSurface, target_min_image_count: u32
    ) -> Result<Arc<Self>, VulkanUniversalError> {
        let instance = device.instance();
        let initialized = device.initialized()?;

        // Format.
        let available_formats = unsafe {
            surface.ash_surface().get_physical_device_surface_formats(
                device.physical_device(), surface.inner()
            )
        }?;

        if available_formats.is_empty() {
            return Err(WindowNotSupportedError::with_entry_str(
                "Graphics device and surface does not have compatible formats."
            ).into())
        }

        let format = Self::choose_surface_format(available_formats);

        // Present modes.
        let available_present_modes = unsafe {
            surface.ash_surface().get_physical_device_surface_present_modes(
                device.physical_device(), surface.inner()
            )
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
                surface.ash_surface().get_physical_device_surface_support(
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
            device: device.clone(),
            swapchain: Weak::default(),
            surface,
            ash_swapchain: khr::Swapchain::new(instance.inner(), initialized.vulkan_device()),
            ash_swapchain_mutex: Mutex::new(()),
            queue_family,
        });

        let swapchain = Arc::new(Self {
            shared: shared.clone(),
            dynamic: ArcCell::new(Arc::new(SwapchainSharedDynamic {
                shared: shared.clone(),
                is_old: Cell::new(false),
                inner: unsafe { mem::zeroed() },
                extent: vk::Extent2D::default(),
                used_min_image_count: 0,
                image_views: ManuallyDrop::new(Vec::with_capacity(0)),
                image_available_semaphores: Vec::with_capacity(0),
                render_finished_semaphores: Vec::with_capacity(0),
            })),
            format,
            present_mode,
            pass: AtomicCell::new(None),
            pass_creation_mutex: Mutex::new(()),
            device: device.clone()
        });

        let reference = unsafe {
            &mut *(Arc::as_ptr(&shared) as *mut SwapchainShared)
        };
        reference.swapchain = Arc::downgrade(&swapchain);

        swapchain.recreate(Some(target_min_image_count))?;

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

    pub fn change_min_image_count(&self, target_count: u32) -> Result<u32, VulkanUniversalError> {
        if target_count != self.dynamic.get().used_min_image_count {
            self.recreate(Some(target_count))?;
        }
        Ok(self.dynamic.get().used_min_image_count)
    }

    pub fn get_swapchain_pass(
        &'init self, render_pass: &Arc<RenderPass<'init>>
    ) -> Result<Arc<SwapchainPass<'init, 'fam>>, VulkanUniversalError> {
        // Check if pass is created.
        match self.pass.get() {
            Some(current_pass) => {
                if self.compare_render_pass(&current_pass, render_pass) {
                    return Ok(current_pass)
                }
            },
            None => (),
        }

        self.create_pass(render_pass)
    }

    pub fn get_swapchain_pass_and_accquire_next_image(
        &'init self, render_pass: &Arc<RenderPass<'init>>, new_fence: &Arc<VulkanFence<'init>>
    ) -> Result<(
            Arc<SwapchainPass<'init, 'fam>>, Arc<VulkanSynchronizedFence<'init>>, usize, u32
        ), VulkanUniversalError> {
        loop {
            let swapchain_pass = self.get_swapchain_pass(render_pass)?;

            let synchronized_fence =
                Arc::new(VulkanSynchronizedFence::new(new_fence.clone()));
            let frame_index = swapchain_pass.next_frame(&synchronized_fence)?;

            match swapchain_pass.accquire_next_image(frame_index) {
                Ok(i) => return Ok((swapchain_pass, synchronized_fence, frame_index, i)),
                Err(result) => {
                    synchronized_fence.retire();
                    match result {
                        SwapchainAccquireNextImageError::Suboptimal => self.recreate(None)?,
                        SwapchainAccquireNextImageError::OutOfDate => self.recreate(None)?,
                        SwapchainAccquireNextImageError::Recreated => (),
                        SwapchainAccquireNextImageError::InvalidOperation(err) =>
                            return Err(err.into()),
                        SwapchainAccquireNextImageError::Vulkan(err) => return Err(err.into()),
                    }
                },
            };
        }
    }

    pub fn recreate(&self, image_count: Option<u32>) -> Result<(), VulkanUniversalError> {
        let old_dynamic = self.dynamic.get();

        // Lock pass creation mutex.
        let _pass_creation_lock = self.lock_pass_creation_mutex()?;

        // Check if swapchain was recreated after pass creation lock.
        if !Arc::ptr_eq(&old_dynamic, &self.dynamic.get()) {
            return Ok(())
        }

        // Wait to frames in flight ends.
        match self.pass.set(None) {
            Some(pass) => {
                for fence in &pass.in_flight_fences {
                    match Weak::upgrade(&fence.get()) {
                        Some(f) => _ = f.wait(),
                        None => (),
                    }
                }
            },
            None => (),
        };

        // Lock swapchain mutex.
        let _swapchain_lock = self.shared.lock_ash_swapchain()?;

        let initialized = self.device.initialized()?;
        let support = SwapchainSupport::new(&self.shared)?;

        let mut family_indices = Vec::new();
        for family in initialized.get_families() {
            family_indices.push(family.index());
        }

        let min_image_count = match image_count {
            Some(c) => c,
            None => self.dynamic.get().used_min_image_count,
        };
        let used_min_image_count = support.get_supported_image_count(min_image_count);

        let create_info = vk::SwapchainCreateInfoKHR {
            s_type: vk::StructureType::SWAPCHAIN_CREATE_INFO_KHR,
            p_next: ptr::null(),
            flags: vk::SwapchainCreateFlagsKHR::empty(),
            surface: self.shared.surface.inner(),
            min_image_count: used_min_image_count,
            image_format: self.format.format,
            image_color_space: self.format.color_space,
            image_extent: support.get_extent(),
            image_array_layers: 1,
            image_usage: vk::ImageUsageFlags::COLOR_ATTACHMENT,
            image_sharing_mode: match family_indices.is_empty() {
                true => vk::SharingMode::CONCURRENT,
                false => vk::SharingMode::EXCLUSIVE,
            },
            queue_family_index_count: family_indices.len() as u32,
            p_queue_family_indices: family_indices.as_ptr(),
            pre_transform: support.get_pre_transform(),
            composite_alpha: vk::CompositeAlphaFlagsKHR::OPAQUE,
            present_mode: self.present_mode,
            clipped: 1,
            old_swapchain: self.dynamic.get().inner
        };

        let inner = unsafe {
            self.shared.ash_swapchain.create_swapchain(&create_info, None)
        }?;

        self.create_dynamic(inner, create_info.image_extent, support, used_min_image_count)?;

        Ok(())
    }

    fn create_dynamic(
        &self, inner: vk::SwapchainKHR, extent: vk::Extent2D, support: SwapchainSupport, used_min_image_count: u32
    ) -> Result<(), VulkanUniversalError> {
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
        let old_dynamic = self.dynamic.get();

        let semaphores_length = image_views.len() - support.capabilities.min_image_count as usize + 1;
        let mut image_available_semaphores = Vec::with_capacity(semaphores_length);
        let mut render_finished_semaphores = Vec::with_capacity(semaphores_length);

        for i in 0..cmp::min(old_dynamic.image_available_semaphores.len(), semaphores_length) {
            image_available_semaphores.push(old_dynamic.image_available_semaphores[i].clone());
            render_finished_semaphores.push(old_dynamic.render_finished_semaphores[i].clone());
        }

        while image_available_semaphores.len() != semaphores_length {
            image_available_semaphores.push(Arc::new(initialized.pool().get_semaphore(&self.device)?));
            render_finished_semaphores.push(Arc::new(initialized.pool().get_semaphore(&self.device)?));
        }

        // Construct.
        old_dynamic.is_old.set(true);

        self.dynamic.set(Arc::new(SwapchainSharedDynamic {
            shared: self.shared.clone(),
            is_old: Cell::new(false),
            inner,
            extent,
            used_min_image_count,
            image_views: ManuallyDrop::new(image_views),
            image_available_semaphores,
            render_finished_semaphores,
        }));

        Ok(())
    }

    fn create_pass(
        &'init self, render_pass: &Arc<RenderPass<'init>>
    ) -> Result<Arc<SwapchainPass<'init, 'fam>>, VulkanUniversalError> {
        // Lock mutex.
        let _lock = self.lock_pass_creation_mutex()?;

        let old_pass = self.pass.get();
        match &old_pass {
            Some(current_pass) => {
                if self.compare_render_pass(current_pass, render_pass) {
                    return Ok(current_pass.clone())
                }
            },
            None => (),
        }

        // Create new pass.
        let dynamic = self.dynamic.get();

        let mut framebuffers = Vec::with_capacity(dynamic.image_views.len());
        let device = self.shared.device();

        for image_view in dynamic.image_views.deref() {
            if render_pass.depth_testing() {
                let image =
                    Arc::new(VulkanImage::new(device, VulkanImageCreateInfo {
                        flags: vk::ImageCreateFlags::empty(),
                        image_type: vk::ImageType::TYPE_2D,
                        extent: vk::Extent3D {
                            width: dynamic.extent.width,
                            height: dynamic.extent.height,
                            depth: 1
                        },
                        format: render_pass.depth_stencil_format(),
                        mip_levels: 1,
                        array_layers: 1,
                        sample_count: render_pass.depth_stencil_sample_count().as_raw(),
                        linear: false,
                        usage: vk::ImageUsageFlags::DEPTH_STENCIL_ATTACHMENT,
                        concurrent: true,
                        layout: vk::ImageLayout::UNDEFINED,
                    }
                )?);

                let attachments = &[SwapchainFramebufferAttachment {
                    image,
                    create_info: VulkanImageViewCreateInfo {
                        flags: vk::ImageViewCreateFlags::empty(),
                        view_type: vk::ImageViewType::TYPE_2D,
                        components: vk::ComponentMapping::default(),
                        subresource_range: vk::ImageSubresourceRange {
                            aspect_mask: vk::ImageAspectFlags::DEPTH,
                            base_mip_level: 0,
                            level_count: 1,
                            base_array_layer: 0,
                            layer_count: 1,
                        },
                    },
                }];

                framebuffers.push(SwapchainFramebuffer::new(&render_pass, image_view, dynamic.extent, attachments)?);
            } else {
                framebuffers.push(SwapchainFramebuffer::new(
                    &render_pass, image_view, dynamic.extent, &[]
                )?);
            }
        }

        let in_flight_fences_length = dynamic.image_available_semaphores.len();
        let mut in_flight_fences =
            Vec::with_capacity(in_flight_fences_length);
        let frame_index;

        match old_pass {
            Some(p) => {
                for i in 0..cmp::min(p.in_flight_fences.len(), in_flight_fences_length) {
                    in_flight_fences.push(p.in_flight_fences[i].clone());
                }
                frame_index = p.frame_index.load(Ordering::Relaxed);
            },
            None => frame_index = 0
        };

        while in_flight_fences.len() != in_flight_fences_length {
            in_flight_fences.push(Rc::new(WeakCell::new(Weak::new())));
        }

        let new_pass = Arc::new(SwapchainPass {
            shared: self.shared.clone(),
            dynamic,
            framebuffers: ManuallyDrop::new(framebuffers),
            in_flight_fences,
            frame_index: AtomicUsize::new(frame_index),
            render_pass: render_pass.clone(),
        });

        self.pass.set(Some(new_pass.clone()));
        Ok(new_pass)
    }

    fn compare_render_pass(
        &self, current_pass: &Arc<SwapchainPass<'init, 'fam>>, render_pass: &Arc<RenderPass<'init>>
    ) -> bool {
        Arc::ptr_eq(&current_pass.render_pass, render_pass) && !current_pass.dynamic.is_old.get()
    }

    fn lock_pass_creation_mutex(&self) -> Result<MutexGuard<()>, InvalidOperationError> {
        match self.pass_creation_mutex.lock() {
            Ok(l) => Ok(l),
            Err(_) => Err(InvalidOperationError::with_str("Another thread holding the mutex panicked.")),
        }
    }
}

pub struct SwapchainShared<'init: 'fam, 'fam> {
    queue_family: &'fam VulkanQueueFamily<'init>,
    device: Arc<VulkanDevice<'init>>,
    surface: VulkanSurface,
    swapchain: Weak<Swapchain<'init, 'fam>>,
    ash_swapchain: khr::Swapchain,
    ash_swapchain_mutex: Mutex<()>,
}

impl<'init: 'fam, 'fam> SwapchainShared<'init, 'fam> {
    pub fn device(&self) -> &Arc<VulkanDevice<'init>> {
        &self.device
    }

    pub fn surface(&self) -> &VulkanSurface {
        &self.surface
    }

    pub fn swapchain(&self) -> &Weak<Swapchain<'init, 'fam>> {
        &self.swapchain
    }

    pub fn lock_ash_swapchain(&self) -> Result<MutexGuard<()>, InvalidOperationError>  {
        match self.ash_swapchain_mutex.lock() {
            Ok(l) => Ok(l),
            Err(_) => Err(InvalidOperationError::with_str("Another thread holding the mutex panicked.")),
        }
    }
}

struct SwapchainSharedDynamic<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    is_old: Cell<bool>,
    inner: vk::SwapchainKHR,
    extent: vk::Extent2D,
    used_min_image_count: u32,
    image_views: ManuallyDrop<Vec<Arc<SwapchainImageView<'init>>>>,
    image_available_semaphores: Vec<Arc<VulkanSemaphore<'init>>>,
    render_finished_semaphores: Vec<Arc<VulkanSemaphore<'init>>>
}

impl Drop for SwapchainSharedDynamic<'_, '_> {
    fn drop(&mut self) {
        unsafe {
            ManuallyDrop::drop(&mut self.image_views);
            self.shared.ash_swapchain.destroy_swapchain(self.inner, None);
        }
    }
}

pub struct SwapchainPass<'init: 'fam, 'fam> {
    shared: Arc<SwapchainShared<'init, 'fam>>,
    dynamic: Arc<SwapchainSharedDynamic<'init, 'fam>>,
    framebuffers: ManuallyDrop<Vec<SwapchainFramebuffer<'init, 'fam>>>,
    in_flight_fences: Vec<Rc<WeakCell<VulkanSynchronizedFence<'init>>>>,
    frame_index: AtomicUsize,
    render_pass: Arc<RenderPass<'init>>
}

impl<'init: 'fam, 'fam> SwapchainPass<'init, 'fam> {
    pub fn next_frame(&self, new_fence: &Arc<VulkanSynchronizedFence<'init>>) -> Result<usize, VulkanUniversalError> {
        let frame_index =
            self.frame_index.fetch_add(1, Ordering::Relaxed) % self.in_flight_fences.len();

        let old_fence =
            self.in_flight_fences[frame_index].set(Arc::downgrade(&new_fence));

        match Weak::upgrade(&old_fence) {
            Some(old_fence_arc) => _ = old_fence_arc.wait()?,
            None => (),
        }

        Ok(frame_index)
    }

    pub fn accquire_next_image(&self, frame_index: usize) -> Result<u32, SwapchainAccquireNextImageError> {
        let result = {
            let semaphore = self.get_image_available_semaphore(frame_index).inner();

            // Lock mutex.
            let _swapchain_lock = self.lock_ash_swapchain()?;

            if self.dynamic.is_old.get() {
                return Err(SwapchainAccquireNextImageError::Recreated)
            }

            unsafe {
                self.shared.ash_swapchain.acquire_next_image(
                    self.inner(), u64::MAX, semaphore, vk::Fence::null()
                )
            }
        };

        match result {
            Ok((index, suboptimal)) => {
                if suboptimal {
                    Err(SwapchainAccquireNextImageError::Suboptimal)
                } else {
                    Ok(index)
                }
            },
            Err(err) => match err {
                vk::Result::ERROR_OUT_OF_DATE_KHR => Err(SwapchainAccquireNextImageError::Suboptimal),
                _ => Err(SwapchainAccquireNextImageError::Vulkan(err))
            },
        }
    }

    pub fn get_framebuffer(&self, index: u32) -> &SwapchainFramebuffer<'init, 'fam> {
        &self.framebuffers[index as usize]
    }

    pub fn get_image_available_semaphore(&self, frame_index: usize) -> &VulkanSemaphore<'init> {
        &self.dynamic.image_available_semaphores[frame_index]
    }

    pub fn get_render_finished_semaphore(&self, frame_index: usize) -> &VulkanSemaphore<'init> {
        &self.dynamic.render_finished_semaphores[frame_index]
    }

    pub fn inner(&self) -> vk::SwapchainKHR {
        self.dynamic.inner
    }

    pub fn swapchain(&self) -> &Weak<Swapchain<'init, 'fam>> {
        &self.shared.swapchain
    }

    pub fn lock_ash_swapchain(&self) -> Result<MutexGuard<()>, InvalidOperationError> {
        self.shared.lock_ash_swapchain()
    }

    pub fn ash_swapchain(&self) -> &khr::Swapchain {
        &self.shared.ash_swapchain
    }

    pub fn present_family(&self) -> &'fam VulkanQueueFamily<'init> {
        self.shared.queue_family
    }
}

impl Drop for SwapchainPass<'_, '_> {
    fn drop(&mut self) {
        for fence in &self.in_flight_fences {
            match Weak::upgrade(&fence.get()) {
                Some(arc) => _ = arc.wait(),
                None => (),
            }
        }

        unsafe {
            ManuallyDrop::drop(&mut self.framebuffers);
        }
    }
}
