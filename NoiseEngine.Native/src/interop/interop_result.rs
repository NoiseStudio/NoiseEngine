use std::{
    error::Error,
    mem::{ManuallyDrop, MaybeUninit},
};

use super::result_error::ResultError;

#[repr(C)]
pub struct InteropResult<T> {
    is_ok: bool,
    ok: MaybeUninit<ManuallyDrop<T>>,
    err: MaybeUninit<ManuallyDrop<ResultError>>,
}

impl<T> InteropResult<T> {
    pub fn with_ok(value: T) -> InteropResult<T> {
        InteropResult {
            is_ok: true,
            ok: MaybeUninit::new(ManuallyDrop::new(value)),
            err: MaybeUninit::uninit(),
        }
    }

    pub fn with_err(err: ResultError) -> InteropResult<T> {
        InteropResult {
            is_ok: false,
            ok: MaybeUninit::uninit(),
            err: MaybeUninit::new(ManuallyDrop::new(err)),
        }
    }
}

impl<T, E: Error + 'static> From<Result<T, E>> for InteropResult<T> {
    fn from(result: Result<T, E>) -> Self {
        match result {
            Ok(ok) => InteropResult::with_ok(ok),
            Err(err) => InteropResult::with_err(ResultError::new(&err)),
        }
    }
}

impl<T> From<Result<T, ResultError>> for InteropResult<T> {
    fn from(result: Result<T, ResultError>) -> Self {
        match result {
            Ok(ok) => InteropResult::with_ok(ok),
            Err(err) => InteropResult::with_err(err),
        }
    }
}
