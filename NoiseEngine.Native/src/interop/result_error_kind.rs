use std::error::Error;

#[repr(C)]
pub enum ResultErrorKind {
    Universal = 0,
    LibraryLoad = 1,
    Overflow = 2,

    GraphicsInstanceCreate = 1000,
    GraphicsOutOfHostMemory = 1001,
    GraphicsOutOfDeviceMemory = 1002
}

impl From<&(dyn Error + 'static)> for ResultErrorKind {
    fn from(_err: &(dyn Error + 'static)) -> Self {
        ResultErrorKind::Universal
    }
}
