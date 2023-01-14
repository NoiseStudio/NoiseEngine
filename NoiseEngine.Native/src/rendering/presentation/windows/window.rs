use std::{ptr, sync::{Arc, Weak}};

use ash::{vk, extensions::khr};
use libc::{c_void, wchar_t};

use crate::{
    errors::{platform::windows::win32::Win32Error, null_reference::NullReferenceError}, interop::prelude::InteropResult,
    logging::log,
    rendering::{
        presentation::window::Window,
        vulkan::{surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError}
    }
};

use super::wnd_class_w::WndClassW;

fn wide_null(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}

pub struct WindowWindows {
    weak: Weak<Self>,
    h_wnd: *mut c_void
}

impl WindowWindows {
    pub fn new(title: &str, width: u32, height: u32) -> InteropResult<Box<Arc<dyn Window>>> {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        let wide_title = wide_null(title).as_ptr();

        // Create window class.
        let window_class = WndClassW {
            lpfn_wnd_proc: Some(DefWindowProcW),
            h_instance,
            lpsz_class_name: wide_title,
            ..Default::default()
        };

        if unsafe { RegisterClassW(&window_class) } == 0 {
            match Win32Error::get_last() {
                Some(err) => return InteropResult::with_err(err.into()),
                None => (),
            }
        }

        // Create window.
        let h_wnd = unsafe {
            CreateWindowExW(
                0,
                wide_title,
                wide_title,
                0,
                300,
                300,
                width as i32,
                height as i32,
                ptr::null_mut(),
                ptr::null_mut(),
                h_instance,
                ptr::null_mut()
            )
        };

        _ = unsafe {
            ShowWindow(h_wnd, 5)
        };

        // Construct.
        let arc = Arc::new(Self { weak: Weak::default(), h_wnd });
        let reference = unsafe {
            &mut *(Arc::as_ptr(&arc) as *mut WindowWindows)
        };
        reference.weak = Arc::downgrade(&arc);

        InteropResult::with_ok(Box::new(arc))
    }
}

impl Drop for WindowWindows {
    fn drop(&mut self) {
        let result = unsafe {
            DestroyWindow(self.h_wnd)
        };

        if result == 0 {
            match Win32Error::get_last() {
                Some(err) => log::error(err.to_string().as_str()),
                None => (),
            }
        }
    }
}

impl Window for WindowWindows {
    fn create_vulkan_surface(&self, instance: &Arc<VulkanInstance>) -> Result<VulkanSurface, VulkanUniversalError> {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        let create_info = vk::Win32SurfaceCreateInfoKHR {
            s_type: vk::StructureType::WIN32_SURFACE_CREATE_INFO_KHR,
            p_next: ptr::null(),
            flags: vk::Win32SurfaceCreateFlagsKHR::empty(),
            hinstance: h_instance,
            hwnd: self.h_wnd,
        };

        let creator = khr::Win32Surface::new(instance.library(), instance.inner());
        let inner = unsafe {
            creator.create_win32_surface(&create_info, None)
        }?;

        let window_arc = match Weak::upgrade(&self.weak) {
            Some(a) => a,
            None => return Err(NullReferenceError::with_str("WindowWindows weak is null.").into()),
        };

        Ok(VulkanSurface::new(instance.clone(), window_arc, inner))
    }
}

#[link(name = "kernel32")]
extern "system" {
    fn GetModuleHandleW(lp_module_name: *const wchar_t) -> *mut c_void;

    fn RegisterClassW(lp_wnd_class: *const WndClassW) -> u16;

    fn CreateWindowExW(
        dw_ex_style: u32, lp_class_name: *const wchar_t, lp_window_name: *const wchar_t, dw_style: u32, x: i32, y: i32,
        n_width: i32, n_height: i32, h_wnd_parent: *mut c_void, h_menu: *mut c_void, h_instance: *mut c_void,
        lp_param: *mut c_void
    ) -> *mut c_void;
}

#[link(name = "user32")]
extern "system" {
    fn DefWindowProcW(h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize) -> isize;

    fn ShowWindow(h_wnd: *mut c_void, n_cmd_show: u32) -> u32;

    fn DestroyWindow(h_wnd: *mut c_void) -> u32;
}
