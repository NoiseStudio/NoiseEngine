use vulkano::{VulkanError, device::DeviceCreationError, OomError, memory::{DeviceMemoryError, MemoryMapError}, buffer::BufferCreationError};

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

impl From<DeviceMemoryError> for ResultError {
    fn from(err: DeviceMemoryError) -> Self {
        ResultError::with_kind(&err, match err {
            DeviceMemoryError::OomError(oom) => oom.into(),
            _ => ResultErrorKind::GraphicsUniversal
        })
    }
}

impl From<BufferCreationError> for ResultError {
    fn from(err: BufferCreationError) -> Self {
        match err {
            BufferCreationError::AllocError(err) => err.into(),
            _ => ResultError::with_kind(&err, ResultErrorKind::GraphicsUniversal)
        }
    }
}

impl From<MemoryMapError> for ResultError {
    fn from(err: MemoryMapError) -> Self {
        ResultError::with_kind(&err, match err {
            MemoryMapError::OomError(oom) => oom.into(),
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
