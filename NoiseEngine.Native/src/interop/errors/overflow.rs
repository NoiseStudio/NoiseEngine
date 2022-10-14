use std::{error::Error, fmt::Display};

use crate::interop::prelude::{ResultError, ResultErrorKind};

#[derive(Debug)]
pub struct OverflowError {
    message: String
}

impl OverflowError {
    pub fn new(message: String) -> Self {
        Self { message }
    }

    pub fn with_str(message: &str) -> Self {
        Self::new(message.to_owned())
    }
}

impl Default for OverflowError {
    fn default() -> Self {
        Self { message: "Overflow.".to_string() }
    }
}

impl Error for OverflowError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for OverflowError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}

impl From<OverflowError> for ResultError {
    fn from(err: OverflowError) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::Overflow)
    }
}
