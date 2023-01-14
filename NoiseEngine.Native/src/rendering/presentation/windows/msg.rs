use std::mem;

use libc::c_void;

#[repr(C)]
pub struct Msg {
    h_wnd: *mut c_void,
    message: u32,
    w_param: usize,
    l_param: isize,
    time: u32,
    pt: Point,
    l_private: u32
}

impl Default for Msg {
    fn default() -> Self {
        unsafe { mem::zeroed() }
    }
}

#[repr(C)]
pub struct Point {
    x: i32,
    y: i32
}
