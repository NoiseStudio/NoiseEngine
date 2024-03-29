use std::{error::Error, ptr};

use crate::interop::prelude::InteropString;

use super::result_error_kind::ResultErrorKind;

#[repr(C)]
pub struct ResultError {
    message: InteropString,
    source_pointer: *const ResultError,
    kind: ResultErrorKind,
}

impl ResultError {
    pub fn new(err: &(dyn Error + 'static)) -> ResultError {
        Self::with_kind(err, ResultErrorKind::from(err))
    }

    pub fn with_kind(err: &(dyn Error + 'static), kind: ResultErrorKind) -> ResultError {
        let source_pointer = match err.source() {
            Some(s) => {
                let b = Box::new(ResultError::new(s));
                Box::into_raw(b)
            }
            None => ptr::null(),
        };

        ResultError {
            message: err.to_string().into(),
            source_pointer,
            kind,
        }
    }
}

impl Drop for ResultError {
    fn drop(&mut self) {
        if !self.source_pointer.is_null() {
            unsafe { Box::from_raw(self.source_pointer as *mut ResultError) };
        }
    }
}
