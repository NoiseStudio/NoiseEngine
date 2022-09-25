use std::mem;

use super::interop_array::InteropArray;

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
        // Unwrap is safe because the string is guaranteed to be valid UTF-8.
        String::from_utf8(interop_string.array.into()).unwrap()
    }
}

impl From<&str> for InteropString {
    fn from(string: &str) -> InteropString {
        InteropString {
            array: Vec::from(string.as_bytes()).into(),
        }
    }
}
