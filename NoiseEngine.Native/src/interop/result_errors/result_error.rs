use std::{error::Error, ptr};

use crate::interop::prelude::{InteropString, InteropResult};

use super::{result_error_trait::ResultErrorTrait, result_error_kind::ResultErrorKind};

#[repr(C)]
pub struct ResultError {
    kind: ResultErrorKind,
    message: InteropString,
    source_pointer: *const ResultError
}

impl ResultError {
    fn new<T: Error + 'static>(err: &T) -> ResultError {
        let source_pointer = match err.source() {
            Some(s) => {
                match s.downcast_ref::<T>() {
                    Some(r) => {
                        let b = Box::new(ResultError::new(r));
                        Box::into_raw(b)
                    }
                    None => ptr::null()
                }
            }
            None => ptr::null()
        };

        ResultError {
            kind: ResultErrorKind::from_err::<T>(),
            message: err.to_string().into(),
            source_pointer
        }
    }
}

impl ResultErrorTrait for ResultError {
}

impl<T, E: Error + 'static> From<Result<T, E>> for InteropResult<T, ResultError> {
    fn from(result: Result<T, E>) -> Self {
        match result {
            Ok(ok) => InteropResult::new(ok),
            Err(err) => InteropResult::with_err(ResultError::new(&err))
        }
    }
}
