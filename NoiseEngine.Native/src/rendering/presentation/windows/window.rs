use std::{ptr, sync::{Arc, Weak}};

use ash::{vk, extensions::khr};
use libc::{c_void, wchar_t};
use uuid::Uuid;

use crate::{
    errors::{
        platform::{windows::win32::Win32Error, platform_universal::PlatformUniversalError},
        null_reference::NullReferenceError
    },
    logging::log,
    rendering::{
        presentation::{window::Window, window_settings::{WindowSettings, WindowMode, WindowControls, WindowCoordinateMode}},
        vulkan::{surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError}
    }
};

use super::{wnd_class_w::WndClassW, rect::Rect};

fn wide_null(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}

pub struct WindowWindows {
    weak: Weak<Self>,
    h_wnd: *mut c_void,
    class_name: Vec<u16>,
    width: u32,
    height: u32,
    settings: WindowSettings
}

impl WindowWindows {
    pub fn new(
        title: &str, width: u32, height: u32, settings: WindowSettings
    ) -> Result<Arc<dyn Window>, PlatformUniversalError> {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        // Create window class.
        let class_name = wide_null(Uuid::new_v4().to_string().as_str());

        let window_class = WndClassW {
            lpfn_wnd_proc: Some(DefWindowProcW),
            h_instance,
            lpsz_class_name: class_name.as_ptr(),
            ..Default::default()
        };

        if unsafe { RegisterClassW(&window_class) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        // Construct.
        let arc = Arc::new(Self {
            weak: Weak::default(), h_wnd: ptr::null_mut(), class_name, width, height, settings
        });
        let reference = unsafe {
            &mut *(Arc::as_ptr(&arc) as *mut WindowWindows)
        };
        reference.weak = Arc::downgrade(&arc);

        // Create window.
        let adjust = reference.get_adjust()?;
        let (position_x, position_y) = reference.get_winapi_position(&adjust);
        let wide_title = wide_null(title).as_ptr();

        reference.h_wnd = unsafe {
            CreateWindowExW(
                0,
                reference.class_name.as_ptr(),
                wide_title,
                reference.get_window_style(),
                position_x,
                position_y,
                adjust.right - adjust.left,
                adjust.bottom - adjust.top,
                ptr::null_mut(),
                ptr::null_mut(),
                h_instance,
                ptr::null_mut()
            )
        };

        _ = unsafe {
            ShowWindow(reference.h_wnd, 5)
        };

        Ok(arc)
    }

    fn get_h_instance() -> *mut c_void {
        unsafe {
            GetModuleHandleW(ptr::null_mut())
        }
    }

    // https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
    fn get_window_style(&self) -> u32 {
        let mut result = match self.settings.mode {
            WindowMode::Windowed => {
                let mut a = 0x00C00000 | 0x00080000; // WS_CAPTION | WS_SYSMENU

                if self.settings.controls.contains(WindowControls::MinimizeButton) {
                    a |= 0x00020000; // WS_MINIMIZEBOX
                }
                if self.settings.controls.contains(WindowControls::MaximizeButton) {
                    a |= 0x00010000; // WS_MAXIMIZEBOX
                }

                a
            },
            WindowMode::Borderless => 0x80000000, // WS_POPUP
        };

        if self.settings.resizable {
            result |= 0x00040000; // WS_THICKFRAME
        }

        result
    }

    fn get_adjust(&self) -> Result<Rect, Win32Error> {
        let rect = Rect {
            left: 0,
            top: 0,
            right: self.width as i32,
            bottom: self.height as i32,
        };

        let result = unsafe {
            AdjustWindowRectEx(&rect, self.get_window_style(), 0, 0)
        };

        if result == 0 {
            return Err(Win32Error::get_last())
        }

        Ok(rect)
    }

    fn get_winapi_position(&self, adjust: &Rect) -> (i32, i32) {
        const CW_USEDEFAULT: u32 = 0x80000000;

        let x = match self.settings.position.x.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => self.settings.position.x.value + adjust.left,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        let y = match self.settings.position.y.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => self.settings.position.y.value + adjust.top,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        (x, y)
    }
}

impl Drop for WindowWindows {
    fn drop(&mut self) {
        let mut result = unsafe {
            DestroyWindow(self.h_wnd)
        };

        if result == 0 {
            log::error(Win32Error::get_last().to_string().as_str());
        }

        result = unsafe {
            UnregisterClassW(self.class_name.as_ptr(), Self::get_h_instance())
        };

        if result == 0 {
            log::error(Win32Error::get_last().to_string().as_str());
        }
    }
}

impl Window for WindowWindows {
    fn create_vulkan_surface(&self, instance: &Arc<VulkanInstance>) -> Result<VulkanSurface, VulkanUniversalError> {
        let create_info = vk::Win32SurfaceCreateInfoKHR {
            s_type: vk::StructureType::WIN32_SURFACE_CREATE_INFO_KHR,
            p_next: ptr::null(),
            flags: vk::Win32SurfaceCreateFlagsKHR::empty(),
            hinstance: Self::get_h_instance(),
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

    fn get_width(&self) -> u32 {
        self.width
    }

    fn get_height(&self) -> u32 {
        self.height
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
    fn UnregisterClassW(lp_class_name: *const wchar_t, h_instance: *mut c_void) -> u32;

    fn DestroyWindow(h_wnd: *mut c_void) -> u32;

    fn DefWindowProcW(h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize) -> isize;

    fn ShowWindow(h_wnd: *mut c_void, n_cmd_show: u32) -> u32;

    fn AdjustWindowRectEx(lp_rect: &Rect, dw_style: u32, b_menu: u32, dw_ex_style: u32) -> u32;
}
