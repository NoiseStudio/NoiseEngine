use std::{ptr, sync::{Arc, Weak}, cell::UnsafeCell};

use ash::{vk, extensions::khr};
use cgmath::Vector2;
use libc::{c_void, wchar_t};
use uuid::Uuid;

use crate::{
    errors::{
        platform::{windows::win32::Win32Error, platform_universal::PlatformUniversalError},
        null_reference::NullReferenceError
    },
    logging::log,
    rendering::{
        presentation::{
            window::Window, window_settings::{WindowSettings, WindowMode, WindowControls, WindowCoordinateMode},
            window_event_handler::WindowEventHandler
        },
        vulkan::{surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError}
    }, platform::windows::rect::Rect
};

use super::{wnd_class_w::WndClassW, msg::Msg};

const PROP_NAME: &[u16] = const_utf16::encode!("NEwp\0");

fn wide_null(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}

#[inline]
fn low_word(l: isize) -> u16 {
    (l & 0xffff) as u16
}
#[inline]
fn high_word(l: isize) -> u16 {
    ((l >> 16) & 0xffff) as u16
}

pub struct WindowWindows {
    id: u64,
    weak: Weak<Self>,
    h_wnd: *mut c_void,
    class_name: Vec<u16>,
    data: UnsafeCell<WindowWindowsData>
}

impl WindowWindows {
    pub fn new(
        id: u64, title: &str, width: u32, height: u32, settings: WindowSettings
    ) -> Result<Arc<dyn Window>, PlatformUniversalError> {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        // Create window class.
        let class_name = wide_null(Uuid::new_v4().to_string().as_str());

        let window_class = WndClassW {
            lpfn_wnd_proc: Some(window_procedure),
            h_instance,
            lpsz_class_name: class_name.as_ptr(),
            ..Default::default()
        };

        if unsafe { RegisterClassW(&window_class) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        // Construct.
        let arc = Arc::new(Self {
            id,
            weak: Weak::default(),
            h_wnd: ptr::null_mut(),
            class_name,
            data: UnsafeCell::new(WindowWindowsData { width, height, settings })
        });

        let reference = unsafe {
            &mut *(Arc::as_ptr(&arc) as *mut WindowWindows)
        };
        reference.weak = Arc::downgrade(&arc);

        // Create window.
        let adjust = reference.get_adjust(Rect {
            left: 0,
            top: 0,
            right: reference.get_width() as i32,
            bottom: reference.get_height() as i32,
        })?;
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

        let h_data = reference as *const WindowWindows as *const c_void;
        if unsafe { SetPropW(reference.h_wnd, PROP_NAME.as_ptr(), h_data) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        _ = unsafe {
            ShowWindow(reference.h_wnd, 1)
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
        let settings = self.data().settings;

        let mut result = match settings.mode {
            WindowMode::Windowed => {
                let mut a = 0x00C00000 | 0x00080000; // WS_CAPTION | WS_SYSMENU

                if settings.controls.contains(WindowControls::MinimizeButton) {
                    a |= 0x00020000; // WS_MINIMIZEBOX
                }
                if settings.controls.contains(WindowControls::MaximizeButton) {
                    a |= 0x00010000; // WS_MAXIMIZEBOX
                }

                a
            },
            WindowMode::Borderless => 0x80000000, // WS_POPUP
        };

        if settings.resizable {
            result |= 0x00040000; // WS_THICKFRAME
        }

        result
    }

    fn get_adjust(&self, rect: Rect) -> Result<Rect, Win32Error> {
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

        let settings = self.data().settings;

        let x = match settings.position.x.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => settings.position.x.value + adjust.left,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        let y = match settings.position.y.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => settings.position.y.value + adjust.top,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        (x, y)
    }

    fn data(&self) -> &WindowWindowsData {
        unsafe { &*self.data.get() }
    }

    fn data_mut(&self) -> &mut WindowWindowsData {
        unsafe { &mut *self.data.get() }
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
    fn get_width(&self) -> u32 {
        self.data().width
    }

    fn get_height(&self) -> u32 {
        self.data().height
    }

    fn pool_events(&self) {
        let mut msg = Msg::default();
        loop {
            let result = unsafe {
                PeekMessageW(&mut msg, self.h_wnd, 0, 0, 1)
            };

            if result == 0 {
                return;
            }

            unsafe {
                TranslateMessage(&msg);
                DispatchMessageW(&msg);
            }
        }
    }

    fn hide(&self) {
        _ = unsafe {
            ShowWindow(self.h_wnd, 0)
        };
    }

    fn set_position(
        &self, position: Option<Vector2<i32>>, size: Option<Vector2<u32>>
    ) -> Result<(), PlatformUniversalError> {
        let mut flags = 0x0004; // SWP_NOZORDER

        let x;
        let y;
        match position {
            Some(p) => {
                x = p.x;
                y = p.y;
            },
            None => {
                flags |= 0x0002; // SWP_NOMOVE
                x = 0;
                y = 0;
            },
        }

        let cx;
        let cy;
        match size {
            Some(s) => {
                let adjust = self.get_adjust(Rect {
                    left: 0,
                    top: 0,
                    right: s.x as i32,
                    bottom: s.y as i32,
                })?;

                cx = adjust.right - adjust.left;
                cy = adjust.bottom - adjust.top;
            },
            None => {
                flags |= 0x0001; // SWP_NOSIZE
                cx = 0;
                cy = 0;
            },
        }

        if unsafe { SetWindowPos(self.h_wnd, ptr::null_mut(), x, y, cx, cy, flags) } == 0 {
            Err(Win32Error::get_last().into())
        } else {
            Ok(())
        }
    }

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
}

unsafe extern "system" fn window_procedure(
    h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize
) -> isize {
    let event_handler = WindowEventHandler::get();
    let window = &*(GetPropW(h_wnd, PROP_NAME.as_ptr()) as *const WindowWindows);
    let data = window.data_mut();

    match msg {
        // WM_SIZE
        0x0005 => {
            let width = low_word(l_param) as u32;
            let height = high_word(l_param) as u32;

            if window.get_width() != width || window.get_height() != height {
                data.width = width;
                data.height = height;
                (event_handler.size_changed)(window.id, width, height);
            }
        },
        // WM_CLOSE
        0x0010 => (event_handler.user_closed)(window.id),
        _ => return DefWindowProcW(h_wnd, msg, w_param, l_param)
    };
    0
}

struct WindowWindowsData {
    pub width: u32,
    pub height: u32,
    pub settings: WindowSettings
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

    fn SetPropW(h_wnd: *mut c_void, lp_string: *const wchar_t, h_data: *const c_void) -> u32;

    fn GetPropW(h_wnd: *mut c_void, lp_string: *const wchar_t) -> *const c_void;

    fn DefWindowProcW(h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize) -> isize;

    fn PeekMessageW(
        lp_msg: &mut Msg, h_wnd: *mut c_void, w_msg_filter_min: u32, w_msg_filter_max: u32, w_remove_msg: u32
    ) -> u32;

    fn TranslateMessage(lp_msg: &Msg) -> u32;

    fn DispatchMessageW(lp_msg: &Msg) -> isize;

    fn ShowWindow(h_wnd: *mut c_void, n_cmd_show: u32) -> u32;

    fn AdjustWindowRectEx(lp_rect: &Rect, dw_style: u32, b_menu: u32, dw_ex_style: u32) -> u32;

    fn SetWindowPos(
        h_wnd: *mut c_void, h_wnd_insert_after: *mut c_void, x: i32, y: i32, cx: i32, cy: i32, flags: u32
    ) -> u32;
}
