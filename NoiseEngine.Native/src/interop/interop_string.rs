use std::mem;

use super::{prelude::{InteropArray, InteropResult}, result_errors::result_error_trait::ResultErrorTrait};

#[repr(C)]
pub struct InteropString {
    array: InteropArray<u8>,
}

impl From<String> for InteropString {
    fn from(string: String) -> InteropString {
        let ptr = string.as_ptr() as *mut u8;
        let length = string.len() as i32;

        mem::forget(string);

        let array = unsafe {
            InteropArray::from_raw_parts(ptr, length)
        };

        InteropString {
            array,
        }
    }
}

impl From<InteropString> for String {
    fn from(interop_string: InteropString) -> String {
        // SAFETY: String is guaranteed to be valid UTF-8.
        unsafe {
            String::from_utf8_unchecked(interop_string.array.into())
        }
    }
}

impl From<&str> for InteropString {
    fn from(string: &str) -> InteropString {
        InteropString {
            array: Vec::from(string.as_bytes()).into(),
        }
    }
}

impl ResultErrorTrait for InteropString {
}

impl<T> From<Result<T, InteropString>> for InteropResult<T, InteropString> {
    fn from(result: Result<T, InteropString>) -> Self {
        match result {
            Ok(ok) => InteropResult::new(ok),
            Err(err) => InteropResult::with_err(err)
        }
    }
}

impl<T> From<Result<T, String>> for InteropResult<T, InteropString> {
    fn from(result: Result<T, String>) -> Self {
        match result {
            Ok(ok) => InteropResult::new(ok),
            Err(err) => InteropResult::with_err(err.into())
        }
    }
}

impl<T> From<Result<T, &str>> for InteropResult<T, InteropString> {
    fn from(result: Result<T, &str>) -> Self {
        match result {
            Ok(ok) => InteropResult::new(ok),
            Err(err) => InteropResult::with_err(err.into())
        }
    }
}
