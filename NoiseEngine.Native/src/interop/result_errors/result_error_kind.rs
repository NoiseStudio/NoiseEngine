use std::error::Error;

#[repr(C)]
pub enum ResultErrorKind {
    Universal = 0
}

impl ResultErrorKind {
    pub fn from_err(_err: &(dyn Error + 'static)) -> ResultErrorKind {
        ResultErrorKind::Universal
    }
}
