use vulkano::{VulkanError, device::DeviceCreationError, OomError};

use crate::interop::prelude::{ResultError, ResultErrorKind};

impl From<VulkanError> for ResultError {
    fn from(err: VulkanError) -> Self {
        ResultError::with_kind(&err, match err {
            VulkanError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            VulkanError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            _ => ResultErrorKind::GraphicsUniversal
        })
    }
}

impl From<DeviceCreationError> for ResultError {
    fn from(err: DeviceCreationError) -> Self {
        ResultError::with_kind(&err, match err {
            DeviceCreationError::DeviceLost => ResultErrorKind::GraphicsDeviceLost,
            DeviceCreationError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            DeviceCreationError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            _ => ResultErrorKind::GraphicsUniversal
        })
    }
}

impl From<OomError> for ResultErrorKind {
    fn from(oom: OomError) -> Self {
        match oom {
            OomError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            OomError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
        }
    }
}
