use std::error::Error;

#[repr(C)]
pub enum ResultErrorKind {
    Universal = 0,
    LibraryLoad = 1,
    InvalidOperation = 2,
    Overflow = 3,

    GraphicsUniversal = 1000,
    GraphicsInstanceCreate = 1001,
    GraphicsOutOfHostMemory = 1002,
    GraphicsOutOfDeviceMemory = 1003,
    GraphicsDeviceLost = 1004
}

impl From<&(dyn Error + 'static)> for ResultErrorKind {
    fn from(_err: &(dyn Error + 'static)) -> Self {
        ResultErrorKind::Universal
    }
}
