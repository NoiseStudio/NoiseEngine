use std::mem::{ManuallyDrop, MaybeUninit};

use super::result_errors::result_error::ResultError;

#[repr(C)]
pub struct InteropResult<T> {
    is_ok: bool,
    ok: MaybeUninit<ManuallyDrop<T>>,
    err: MaybeUninit<ManuallyDrop<ResultError>>
}

impl<T> InteropResult<T> {
    pub fn with_ok(value: T) -> InteropResult<T> {
        InteropResult {
            is_ok: true,
            ok: MaybeUninit::new(ManuallyDrop::new(value)),
            err: MaybeUninit::uninit()
        }
    }

    pub fn with_err(err: ResultError) -> InteropResult<T> {
        InteropResult {
            is_ok: false,
            ok: MaybeUninit::uninit(),
            err: MaybeUninit::new(ManuallyDrop::new(err))
        }
    }
}
