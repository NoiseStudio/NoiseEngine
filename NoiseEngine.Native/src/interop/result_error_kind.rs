use std::error::Error;

#[repr(C)]
pub enum ResultErrorKind {
    Universal = 0
}

impl From<&(dyn Error + 'static)> for ResultErrorKind {
    fn from(_err: &(dyn Error + 'static)) -> Self {
        ResultErrorKind::Universal
    }
}
