use once_cell::sync::OnceCell;

use crate::interop::prelude::InteropReadOnlySpan;

use super::{log_level::LogLevel, log_data::LogData, error::{LoggingError, LoggingErrorKind}};

pub(crate) struct Logger {
    pub(crate) handler: unsafe extern "C" fn(LogData),
}

static INSTANCE: OnceCell<Logger> = OnceCell::new();

pub(crate) fn initialize(logger: Logger) -> Result<(), LoggingError> {
    INSTANCE.set(logger).map_err(|_| LoggingError::new(LoggingErrorKind::AlreadyInitialized))
}

pub(crate) fn terminate() {
}

/// # Panics
/// This function panics if called before [`initialize`].
pub(crate) fn log(level: LogLevel, message: &str) {
    let logger = INSTANCE.get().expect("logger is not initialized");
    let message = InteropReadOnlySpan::from(message.as_bytes());
    logger.log(level, message);
}

/// # Panics
/// This function panics if called before [`initialize`].
pub(crate) fn log_owned(level: LogLevel, message: String) {
    let logger = INSTANCE.get().expect("logger is not initialized");
    let message = InteropReadOnlySpan::from(message.as_bytes());
    logger.log(level, message);
}

impl Logger {
    fn log(&self, level: LogLevel, message: InteropReadOnlySpan<u8>) {
        let log_data = LogData {
            level,
            message,
        };

        // SAFETY: The handler is set by the user of the library and is expected to be safe.
        unsafe {
            (self.handler)(log_data);
        }
    }
}
