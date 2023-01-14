use std::{mem, str};

use super::prelude::InteropArray;

#[repr(C)]
pub struct InteropString {
    array: InteropArray<u8>,
}

impl From<InteropString> for String {
    fn from(interop_string: InteropString) -> String {
        // SAFETY: String is guaranteed to be valid UTF-8.
        unsafe {
            String::from_utf8_unchecked(interop_string.array.into())
        }
    }
}

impl From<&InteropString> for Vec<u8> {
    fn from(interop_string: &InteropString) -> Self {
        interop_string.array.as_slice().to_vec()
    }
}

impl<'a> From<&'a InteropString> for &'a str {
    fn from(interop_string: &'a InteropString) -> Self {
        // SAFETY: String is guaranteed to be valid UTF-8.
        unsafe {
            str::from_utf8_unchecked((&interop_string.array).into())
        }
    }
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

impl From<&str> for InteropString {
    fn from(string: &str) -> InteropString {
        InteropString {
            array: Vec::from(string.as_bytes()).into(),
        }
    }
}
