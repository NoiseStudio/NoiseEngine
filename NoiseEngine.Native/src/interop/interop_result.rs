use std::mem::{ManuallyDrop, MaybeUninit};

use super::result_errors::result_error::ResultError;

#[repr(C)]
pub struct InteropResult<T, E: ResultError> {
    is_ok: bool,
    ok: MaybeUninit<ManuallyDrop<T>>,
    err: MaybeUninit<ManuallyDrop<E>>
}

impl<T, E: ResultError> InteropResult<T, E> {
    pub fn new(value: T) -> InteropResult<T, E> {
        InteropResult {
            is_ok: true,
            ok: MaybeUninit::new(ManuallyDrop::new(value)),
            err: MaybeUninit::uninit()
        }
    }

    pub fn new_err(err: E) -> InteropResult<T, E> {
        InteropResult {
            is_ok: false,
            ok: MaybeUninit::uninit(),
            err: MaybeUninit::new(ManuallyDrop::new(err))
        }
    }
}