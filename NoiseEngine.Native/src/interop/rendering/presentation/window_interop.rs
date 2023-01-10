use std::sync::Arc;

use crate::{
    rendering::presentation::window::Window, interop::prelude::{InteropString, InteropResult},
    errors::invalid_operation::InvalidOperationError
};

#[no_mangle]
#[allow(unused_variables)]
extern "C" fn rendering_presentation_window_interop_create(
    title: InteropString, width: u32, height: u32
) -> InteropResult<Box<Arc<dyn Window>>> {
    #[cfg(target_os = "windows")]
    return crate::rendering::presentation::windows::window::WindowWindows::new(
        String::from(title).as_str(), width, height
    );

    #[allow(unreachable_code)]
    InteropResult::with_err(InvalidOperationError::with_str(
        "Window is not supported on this device."
    ).into())
}

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_destroy(_handle: Box<Arc<dyn Window>>) {
}
