use std::{error::Error, ptr};

use crate::interop::prelude::{InteropString, InteropResult};

use super::result_error_kind::ResultErrorKind;

#[repr(C)]
pub struct ResultError {
    message: InteropString,
    source_pointer: *const ResultError,
    kind: ResultErrorKind
}

impl ResultError {
    pub fn new(err: &(dyn Error + 'static)) -> ResultError {
        Self::with_kind(err, ResultErrorKind::from_err(err))
    }

    pub fn with_kind(err: &(dyn Error + 'static), kind: ResultErrorKind) -> ResultError {
        let source_pointer = match err.source() {
            Some(s) => {
                let b = Box::new(ResultError::new(s));
                Box::into_raw(b)
            }
            None => ptr::null()
        };

        ResultError {
            message: err.to_string().into(),
            source_pointer,
            kind
        }
    }
}

impl<T, E: Error + 'static> From<Result<T, E>> for InteropResult<T> {
    fn from(result: Result<T, E>) -> Self {
        match result {
            Ok(ok) => InteropResult::with_ok(ok),
            Err(err) => InteropResult::with_err(ResultError::new(&err))
        }
    }
}

impl<T> From<Result<T, ResultError>> for InteropResult<T> {
    fn from(result: Result<T, ResultError>) -> Self {
        match result {
            Ok(ok) => InteropResult::with_ok(ok),
            Err(err) => InteropResult::with_err(err)
        }
    }
}
