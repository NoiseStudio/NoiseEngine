use vulkano::OomError;

use crate::interop::prelude::ResultErrorKind;

impl From<OomError> for ResultErrorKind {
    fn from(oom: OomError) -> Self {
        match oom {
            OomError::OutOfDeviceMemory => ResultErrorKind::GraphicsOutOfDeviceMemory,
            OomError::OutOfHostMemory => ResultErrorKind::GraphicsOutOfHostMemory,
        }
    }
}
