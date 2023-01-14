use std::{error::Error, fmt::Display};

use crate::interop::prelude::ResultError;

use super::windows::win32::Win32Error;

#[derive(Debug)]
pub enum PlatformUniversalError {
    None,
    #[cfg(target_os = "windows")]
    Windows(Win32Error)
}

impl Error for PlatformUniversalError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            PlatformUniversalError::None => unimplemented!(),
            #[cfg(target_os = "windows")]
            PlatformUniversalError::Windows(err) => err.source()
        }
    }
}

impl Display for PlatformUniversalError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", match self {
            PlatformUniversalError::None => unimplemented!(),
            #[cfg(target_os = "windows")]
            PlatformUniversalError::Windows(err) => err.to_string()
        })
    }
}

#[cfg(target_os = "windows")]
impl From<Win32Error> for PlatformUniversalError {
    fn from(err: Win32Error) -> Self {
        Self::Windows(err)
    }
}

impl From<PlatformUniversalError> for ResultError {
    fn from(err: PlatformUniversalError) -> Self {
        match err {
            PlatformUniversalError::None => unimplemented!(),
            #[cfg(target_os = "windows")]
            PlatformUniversalError::Windows(err) => err.into()
        }
    }
}
