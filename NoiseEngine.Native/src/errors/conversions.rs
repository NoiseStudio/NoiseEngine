use std::str::Utf8Error;

use crate::interop::prelude::{ResultError, ResultErrorKind};

impl From<Utf8Error> for ResultError {
    fn from(err: Utf8Error) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::Universal)
    }
}
