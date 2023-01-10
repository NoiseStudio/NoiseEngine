use crate::{rendering::presentation::windows::window::WindowWindows, interop::prelude::InteropString};

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_create(title: InteropString, width: u32, height: u32) {
    WindowWindows::new(String::from(title).as_str(), width, height);
}
