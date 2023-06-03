use std::{error::Error, fmt::Display, ptr, slice};

use libc::{c_char, c_void, wchar_t};

use crate::interop::prelude::{ResultError, ResultErrorKind};

#[repr(C)]
#[derive(Debug)]
pub struct Win32Error {
    code: u32,
}

impl Win32Error {
    pub fn get_last() -> Self {
        Self {
            code: unsafe { GetLastError() },
        }
    }
}

impl Error for Win32Error {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }
}

impl Display for Win32Error {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        if self.code & (1 << 29) > 0 {
            return write!(f, "Win32ApplicationError({})", self.code);
        }

        let mut buffer: *mut u16 = ptr::null_mut();

        let tchar_count_excluding_null = unsafe {
            FormatMessageW(
                0x00000100 | 0x00001000 | 0x00000200,
                ptr::null_mut(),
                0,
                0,
                &mut buffer as *mut *mut u16 as *mut u16,
                0,
                ptr::null_mut(),
            )
        };

        if tchar_count_excluding_null == 0 || buffer.is_null() {
            return Err(std::fmt::Error);
        }

        let buffer_slice: &[u16] =
            unsafe { slice::from_raw_parts(buffer, tchar_count_excluding_null as usize) };

        for decode_result in core::char::decode_utf16(buffer_slice.iter().copied()) {
            match decode_result {
                Ok('\r') | Ok('\n') => write!(f, " ")?,
                Ok(ch) => write!(f, "{}", ch)?,
                Err(_) => write!(f, "�")?,
            }
        }

        unsafe {
            LocalFree(buffer as *mut c_void);
        }

        Ok(())
    }
}

impl From<Win32Error> for ResultError {
    fn from(err: Win32Error) -> Self {
        ResultError::with_kind(&err, ResultErrorKind::Universal)
    }
}

#[link(name = "kernel32")]
extern "system" {
    fn GetLastError() -> u32;

    fn FormatMessageW(
        dw_flags: u32,
        lp_source: *const c_void,
        dw_message_id: u32,
        dw_language_id: u32,
        lp_buffer: *const wchar_t,
        n_size: u32,
        arguments: *mut c_char,
    ) -> u32;

    fn LocalFree(h_mem: *mut c_void) -> *mut c_void;
}
