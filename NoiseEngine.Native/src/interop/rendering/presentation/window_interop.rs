use std::sync::Arc;

use cgmath::Vector2;

use crate::{
    rendering::presentation::{window::Window, window_settings::WindowSettings},
    interop::prelude::{InteropString, InteropResult, InteropOption}, errors::invalid_operation::InvalidOperationError
};

#[no_mangle]
#[allow(unused_variables)]
extern "C" fn rendering_presentation_window_interop_create(
    id: u64, title: InteropString, width: u32, height: u32, settings: WindowSettings
) -> InteropResult<Box<Arc<dyn Window>>> {
    #[cfg(target_os = "windows")]
    match crate::rendering::presentation::windows::window::WindowWindows::new(
        id, String::from(title), width, height, settings
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
extern "C" fn rendering_presentation_window_interop_destroy(handle: Box<Arc<dyn Window>>) -> InteropResult<()> {
    match handle.dispose() {
        Ok(r) => InteropResult::with_ok(r),
        Err(err) => InteropResult::with_err(err.into()),
    }
}

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_pool_events(window: &Arc<dyn Window>) {
    window.pool_events();
}

#[no_mangle]
extern "C" fn rendering_presentation_window_interop_set_position(
    window: &Arc<dyn Window>, position: InteropOption<Vector2<i32>>, size: InteropOption<Vector2<u32>>
) -> InteropResult<()> {
    match window.set_position(position.into(), size.into()) {
        Ok(()) => InteropResult::with_ok(()),
        Err(err) => InteropResult::with_err(err.into()),
    }
}
