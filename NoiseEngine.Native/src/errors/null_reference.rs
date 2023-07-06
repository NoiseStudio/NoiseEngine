use std::{error::Error, fmt::Display};

use crate::interop::prelude::{ResultError, ResultErrorKind};

#[derive(Debug)]
pub struct NullReferenceError {
    message: String,
}

impl NullReferenceError {
    pub fn new(message: String) -> Self {
        Self { message }
    }

    pub fn with_str(message: &str) -> Self {
        Self::new(message.to_owned())
    }
}

impl Default for NullReferenceError {
    fn default() -> Self {
        Self {
            message: "Object reference not set to an instance of an object.".to_string(),
        }
    }
}

impl Error for NullReferenceError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for NullReferenceError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}

impl From<NullReferenceError> for ResultError {
    fn from(err: NullReferenceError) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::NullReference)
    }
}
