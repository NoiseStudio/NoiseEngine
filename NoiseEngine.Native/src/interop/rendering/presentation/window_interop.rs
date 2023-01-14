use std::sync::Arc;

use crate::{
    rendering::presentation::{window::Window, window_settings::WindowSettings},
    interop::prelude::{InteropString, InteropResult}, errors::invalid_operation::InvalidOperationError
};

#[no_mangle]
#[allow(unused_variables)]
extern "C" fn rendering_presentation_window_interop_create(
    id: u64, title: InteropString, width: u32, height: u32, settings: WindowSettings
) -> InteropResult<Box<Arc<dyn Window>>> {
    #[cfg(target_os = "windows")]
    match crate::rendering::presentation::windows::window::WindowWindows::new(
        id, String::from(title).as_str(), width, height, settings
    ) {
        Ok(w) => return InteropResult::with_ok(Box::new(w)),
        Err(err) => return InteropResult::with_err(err.into())
    }

    #[allow(unreachable_code)]
    InteropResult::with_err(InvalidOperationError::with_str(
        "Window is not supported on this device."
    ).into())
}

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_destroy(handle: Box<Arc<dyn Window>>) {
    handle.hide();
}

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_pool_events(window: &Arc<dyn Window>) {
    window.pool_events();
}
