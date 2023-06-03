use std::{error::Error, fmt::Display};

use ash::vk;

use crate::{
    errors::invalid_operation::InvalidOperationError,
    interop::prelude::{ResultError, ResultErrorKind},
};

#[derive(Debug)]
pub enum SwapchainAccquireNextImageError {
    Suboptimal(u32),
    OutOfDate,
    Recreated,
    InvalidOperation(InvalidOperationError),
    Vulkan(vk::Result),
}

impl Error for SwapchainAccquireNextImageError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            Self::InvalidOperation(err) => err.source(),
            Self::Vulkan(err) => err.source(),
            _ => None,
        }
    }
}

impl Display for SwapchainAccquireNextImageError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "{}",
            match self {
                Self::Suboptimal(_) => "Suboptimal".to_string(),
                Self::OutOfDate => "Out of date".to_string(),
                Self::Recreated => "Recreated".to_string(),
                Self::InvalidOperation(err) => err.to_string(),
                Self::Vulkan(err) => err.to_string(),
            }
        )
    }
}

impl From<InvalidOperationError> for SwapchainAccquireNextImageError {
    fn from(err: InvalidOperationError) -> Self {
        Self::InvalidOperation(err)
    }
}

impl From<vk::Result> for SwapchainAccquireNextImageError {
    fn from(err: vk::Result) -> Self {
        Self::Vulkan(err)
    }
}

impl From<SwapchainAccquireNextImageError> for ResultError {
    fn from(err: SwapchainAccquireNextImageError) -> Self {
        match err {
            SwapchainAccquireNextImageError::InvalidOperation(err) => err.into(),
            SwapchainAccquireNextImageError::Vulkan(err) => err.into(),
            _ => ResultError::with_kind(&err, ResultErrorKind::InvalidOperation),
        }
    }
}
