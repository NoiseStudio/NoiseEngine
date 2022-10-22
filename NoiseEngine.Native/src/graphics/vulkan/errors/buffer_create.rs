use std::{error::Error, fmt::Display};

use vulkano::{buffer::BufferCreationError, memory::DeviceMemoryError};

use crate::{interop::prelude::ResultError, errors::invalid_operation::InvalidOperationError};

#[derive(Debug)]
pub enum VulkanBufferCreateError {
    InvalidOperation(InvalidOperationError),
    BufferCreation(BufferCreationError),
    DeviceMemory(DeviceMemoryError)
}

impl Error for VulkanBufferCreateError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            VulkanBufferCreateError::InvalidOperation(err) => err.source(),
            VulkanBufferCreateError::BufferCreation(err) => err.source(),
            VulkanBufferCreateError::DeviceMemory(err) => err.source(),
        }
    }
}

impl Display for VulkanBufferCreateError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", match self {
            VulkanBufferCreateError::InvalidOperation(err) => err.to_string(),
            VulkanBufferCreateError::BufferCreation(err) => err.to_string(),
            VulkanBufferCreateError::DeviceMemory(err) => err.to_string()
        })
    }
}

impl From<InvalidOperationError> for VulkanBufferCreateError {
    fn from(err: InvalidOperationError) -> Self {
        Self::InvalidOperation(err)
    }
}

impl From<BufferCreationError> for VulkanBufferCreateError {
    fn from(err: BufferCreationError) -> Self {
        Self::BufferCreation(err)
    }
}

impl From<DeviceMemoryError> for VulkanBufferCreateError {
    fn from(err: DeviceMemoryError) -> Self {
        Self::DeviceMemory(err)
    }
}

impl From<VulkanBufferCreateError> for ResultError {
    fn from(err: VulkanBufferCreateError) -> Self {
        match err {
            VulkanBufferCreateError::InvalidOperation(err) => err.into(),
            VulkanBufferCreateError::BufferCreation(err) => err.into(),
            VulkanBufferCreateError::DeviceMemory(err) => err.into()
        }
    }
}
