use once_cell::sync::OnceCell;
use crate::errors::invalid_operation::InvalidOperationError;
use crate::interop::prelude::InteropSpan;

#[repr(C)]
pub(crate) struct AudioListenerEventHandler {
    pub(crate) sample_handler: unsafe extern "C" fn(id: u64, sample_buffer: &mut [f32]),
}

static INSTANCE: OnceCell<AudioListenerEventHandler> = OnceCell::new();

pub(crate) fn initialize(handler: AudioListenerEventHandler) -> Result<(), InvalidOperationError> {
    INSTANCE
        .set(handler)
        .map_err(|_| InvalidOperationError::with_str("AudioListenerEventHandler already initialized."))
}

impl AudioListenerEventHandler {
    /// # Panics
    /// This function panics if called before [`initialize`].
    #[allow(dead_code)]
    pub(crate) fn get() -> &'static AudioListenerEventHandler {
        INSTANCE
            .get()
            .expect("AudioListenerEventHandler is not initialized.")
    }
}
