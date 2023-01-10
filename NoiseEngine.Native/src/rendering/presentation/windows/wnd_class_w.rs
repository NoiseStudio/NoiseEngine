use std::mem;

use libc::{c_void, wchar_t};

#[repr(C)]
pub struct WndClassW {
    pub style: u32,
    pub lpfn_wnd_proc: Option<
        unsafe extern "system" fn(
            hwnd: *mut c_void, u_msg: u32, w_param: usize, l_param: isize
        ) -> isize
    >,
    pub cb_cls_extra: i32,
    pub cb_wnd_extra: i32,
    pub h_instance: *mut c_void,
    pub h_icon: *mut c_void,
    pub h_cursor: *mut c_void,
    pub hbr_background: *mut c_void,
    pub lpsz_menu_name: *const wchar_t,
    pub lpsz_class_name: *const wchar_t
}

impl Default for WndClassW {
    fn default() -> Self {
        unsafe { mem::zeroed() }
    }
}
