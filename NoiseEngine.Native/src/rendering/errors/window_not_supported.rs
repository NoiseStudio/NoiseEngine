use std::{error::Error, fmt::Display};

use crate::interop::prelude::{ResultError, ResultErrorKind};

const DEFAULT_MESSAGE: &str = "Window is not supported on this graphics device.";

#[derive(Debug)]
pub struct WindowNotSupportedError {
    message: String,
}

impl WindowNotSupportedError {
    pub fn new(message: String) -> Self {
        Self { message }
    }

    pub fn with_str(message: &str) -> Self {
        Self::new(message.to_owned())
    }

    pub fn with_entry_str(message: &str) -> Self {
        Self::new(format!("{} {}", DEFAULT_MESSAGE, message))
    }
}

impl Default for WindowNotSupportedError {
    fn default() -> Self {
        Self {
            message: DEFAULT_MESSAGE.to_string(),
        }
    }
}

impl Error for WindowNotSupportedError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for WindowNotSupportedError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}

impl From<WindowNotSupportedError> for ResultError {
    fn from(err: WindowNotSupportedError) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::WindowNotSupported)
    }
}
