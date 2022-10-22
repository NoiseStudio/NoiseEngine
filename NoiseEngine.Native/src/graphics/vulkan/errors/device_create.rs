use std::{error::Error, fmt::Display};

use vulkano::device::DeviceCreationError;

use crate::{interop::prelude::ResultError, errors::invalid_operation::InvalidOperationError};

#[derive(Debug)]
pub enum VulkanDeviceCreateError {
    InvalidOperation(InvalidOperationError),
    DeviceCreation(DeviceCreationError)
}

impl Error for VulkanDeviceCreateError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            VulkanDeviceCreateError::InvalidOperation(err) => err.source(),
            VulkanDeviceCreateError::DeviceCreation(err) => err.source()
        }
    }
}

impl Display for VulkanDeviceCreateError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", match self {
            VulkanDeviceCreateError::InvalidOperation(err) => err.to_string(),
            VulkanDeviceCreateError::DeviceCreation(err) => err.to_string()
        })
    }
}

impl From<InvalidOperationError> for VulkanDeviceCreateError {
    fn from(err: InvalidOperationError) -> Self {
        Self::InvalidOperation(err)
    }
}

impl From<DeviceCreationError> for VulkanDeviceCreateError {
    fn from(err: DeviceCreationError) -> Self {
        Self::DeviceCreation(err)
    }
}

impl From<VulkanDeviceCreateError> for ResultError {
    fn from(err: VulkanDeviceCreateError) -> Self {
        match err {
            VulkanDeviceCreateError::InvalidOperation(err) => err.into(),
            VulkanDeviceCreateError::DeviceCreation(err) => err.into()
        }
    }
}
