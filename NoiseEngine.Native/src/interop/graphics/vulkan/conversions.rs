use vulkano::VulkanError;

use crate::interop::prelude::{ResultError, ResultErrorKind};

impl From<VulkanError> for ResultError {
    fn from(err: VulkanError) -> Self {
        ResultError::with_kind(&err, match err {
            VulkanError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
            VulkanError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            _ => ResultErrorKind::Universal
        })
    }
}