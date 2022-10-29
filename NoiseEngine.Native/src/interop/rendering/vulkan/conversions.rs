use ash::vk;
use gpu_alloc::{AllocationError, MapError};

use crate::interop::prelude::{ResultError, ResultErrorKind};

pub fn from_vk_result(err: vk::Result, default: ResultErrorKind) -> ResultError {
    ResultError::with_kind(&err, match err {
        vk::Result::ERROR_OUT_OF_HOST_MEMORY => ResultErrorKind::GraphicsOutOfHostMemory,
        vk::Result::ERROR_OUT_OF_DEVICE_MEMORY => ResultErrorKind::GraphicsOutOfDeviceMemory,
        vk::Result::ERROR_DEVICE_LOST => ResultErrorKind::GraphicsDeviceLost,
        _ => default
    })
}

impl From<vk::Result> for ResultError {
    fn from(err: vk::Result) -> Self {
        from_vk_result(err, ResultErrorKind::GraphicsUniversal)
    }
}

impl From<AllocationError> for ResultError {
    fn from(err: AllocationError) -> Self {
        ResultError::with_kind(&err, match err {
            AllocationError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            AllocationError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            _ => ResultErrorKind::GraphicsUniversal
        })
    }
}

impl From<MapError> for ResultError {
    fn from(err: MapError) -> Self {
        ResultError::with_kind(&err, match err {
            MapError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            MapError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            _ => ResultErrorKind::GraphicsUniversal
        })
    }
}
