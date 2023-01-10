use std::ptr;

use libc::{c_void, wchar_t};

use super::wnd_class_w::WndClassW;

fn wide_null(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}

pub struct WindowWindows {
}

impl WindowWindows {
    pub fn new(title: &str, width: u32, height: u32) -> Self {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        let wide_title = wide_null(title).as_ptr();

        let mut window_class = WndClassW::default();
        Self::new_window_class(h_instance, wide_title, &mut window_class);

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

        Self {}
    }

    fn new_window_class(h_instance: *mut c_void, wide_title: *const wchar_t, window_class: &mut WndClassW) {
        window_class.lpfn_wnd_proc = Some(DefWindowProcW);
        window_class.h_instance = h_instance;
        window_class.lpsz_class_name = wide_title;

        let atom = unsafe { RegisterClassW(window_class) };
        if atom == 0 {
            let last_error = unsafe { GetLastError() };
            panic!("Could not register the window class, error code: {}", last_error);
        }
    }
}

#[link(name = "kernel32")]
extern "system" {
    fn GetModuleHandleW(lp_module_name: *const wchar_t) -> *mut c_void;

    fn RegisterClassW(lp_wnd_class: *const WndClassW) -> u16;

    fn GetLastError() -> u32;

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
}
