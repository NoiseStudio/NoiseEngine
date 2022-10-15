use std::{error::Error, fmt::Display};

use crate::interop::prelude::{ResultError, ResultErrorKind};

#[derive(Debug)]
pub struct InvalidOperationError {
    message: String
}

impl InvalidOperationError {
    pub fn new(message: String) -> Self {
        Self { message }
    }

    pub fn with_str(message: &str) -> Self {
        Self::new(message.to_owned())
    }
}

impl Default for InvalidOperationError {
    fn default() -> Self {
        Self { message: "Invalid operation.".to_string() }
    }
}

impl Error for InvalidOperationError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for InvalidOperationError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}

impl From<InvalidOperationError> for ResultError {
    fn from(err: InvalidOperationError) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::InvalidOperation)
    }
}
