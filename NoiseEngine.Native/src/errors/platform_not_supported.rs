use std::{error::Error, fmt::Display};

use crate::interop::prelude::{ResultError, ResultErrorKind};

#[derive(Debug)]
pub struct PlatformNotSupportedError {
    message: String,
}

impl PlatformNotSupportedError {
    pub fn new(message: String) -> Self {
        Self { message }
    }

    pub fn with_str(message: &str) -> Self {
        Self::new(message.to_owned())
    }
}

impl Default for PlatformNotSupportedError {
    fn default() -> Self {
        Self {
            message: "Platform not supported.".to_string(),
        }
    }
}

impl Error for PlatformNotSupportedError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for PlatformNotSupportedError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}

impl From<PlatformNotSupportedError> for ResultError {
    fn from(err: PlatformNotSupportedError) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::PlatformNotSupported)
    }
}
