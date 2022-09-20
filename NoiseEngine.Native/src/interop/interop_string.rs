use super::interop_array::InteropArray;

#[repr(C)]
pub struct InteropString {
    array: InteropArray<u8>,
}

impl From<String> for InteropString {
    fn from(string: String) -> InteropString {
        InteropString {
            array: string.into_bytes().into(),
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
            array: string.as_bytes().into(),
        }
    }
}
