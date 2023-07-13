use std::{error::Error, fmt::Display, str::Utf8Error};

use ash::vk;

use crate::{
    errors::{
        invalid_operation::InvalidOperationError, null_reference::NullReferenceError,
        overflow::OverflowError,
    },
    interop::prelude::ResultError,
    rendering::errors::window_not_supported::WindowNotSupportedError,
};

#[derive(Debug)]
pub enum VulkanUniversalError {
    NullReference(NullReferenceError),
    InvalidOperation(InvalidOperationError),
    Overflow(OverflowError),
    Utf8(Utf8Error),
    Vulkan(vk::Result),
    WindowNotSupported(WindowNotSupportedError),
}

impl Error for VulkanUniversalError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            VulkanUniversalError::NullReference(err) => err.source(),
            VulkanUniversalError::InvalidOperation(err) => err.source(),
            VulkanUniversalError::Overflow(err) => err.source(),
            VulkanUniversalError::Utf8(err) => err.source(),
            VulkanUniversalError::Vulkan(err) => err.source(),
            VulkanUniversalError::WindowNotSupported(err) => err.source(),
        }
    }
}

impl Display for VulkanUniversalError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "{}",
            match self {
                VulkanUniversalError::NullReference(err) => err.to_string(),
                VulkanUniversalError::InvalidOperation(err) => err.to_string(),
                VulkanUniversalError::Overflow(err) => err.to_string(),
                VulkanUniversalError::Utf8(err) => err.to_string(),
                VulkanUniversalError::Vulkan(err) => err.to_string(),
                VulkanUniversalError::WindowNotSupported(err) => err.to_string(),
            }
        )
    }
}

impl From<NullReferenceError> for VulkanUniversalError {
    fn from(err: NullReferenceError) -> Self {
        Self::NullReference(err)
    }
}

impl From<InvalidOperationError> for VulkanUniversalError {
    fn from(err: InvalidOperationError) -> Self {
        Self::InvalidOperation(err)
    }
}

impl From<OverflowError> for VulkanUniversalError {
    fn from(err: OverflowError) -> Self {
        Self::Overflow(err)
    }
}

impl From<Utf8Error> for VulkanUniversalError {
    fn from(err: Utf8Error) -> Self {
        Self::Utf8(err)
    }
}

impl From<vk::Result> for VulkanUniversalError {
    fn from(err: vk::Result) -> Self {
        Self::Vulkan(err)
    }
}

impl From<WindowNotSupportedError> for VulkanUniversalError {
    fn from(err: WindowNotSupportedError) -> Self {
        Self::WindowNotSupported(err)
    }
}

impl From<VulkanUniversalError> for ResultError {
    fn from(err: VulkanUniversalError) -> Self {
        match err {
            VulkanUniversalError::NullReference(err) => err.into(),
            VulkanUniversalError::InvalidOperation(err) => err.into(),
            VulkanUniversalError::Overflow(err) => err.into(),
            VulkanUniversalError::Utf8(err) => err.into(),
            VulkanUniversalError::Vulkan(err) => err.into(),
            VulkanUniversalError::WindowNotSupported(err) => err.into(),
        }
    }
}
