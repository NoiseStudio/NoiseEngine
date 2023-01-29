use once_cell::sync::OnceCell;

use crate::errors::invalid_operation::InvalidOperationError;

#[repr(C)]
pub(crate) struct WindowEventHandler {
    pub user_closed: unsafe extern "C" fn(id: u64),
    pub focused: unsafe extern "C" fn(id: u64),
    pub unfocused: unsafe extern "C" fn(id: u64),
    pub size_changed: unsafe extern "C" fn(id: u64, new_width: u32, new_height: u32)
}

static INSTANCE: OnceCell<WindowEventHandler> = OnceCell::new();

pub(crate) fn initialize(handler: WindowEventHandler) -> Result<(), InvalidOperationError> {
    INSTANCE.set(handler).map_err(|_| InvalidOperationError::with_str(
        "WindowEventHandler already initialized."
    ))
}

impl WindowEventHandler {
    /// # Panics
    /// This function panics if called before [`initialize`].
    pub(crate) fn get() -> &'static WindowEventHandler {
        INSTANCE.get().expect("WindowEventHandler is not initialized.")
    }
}
