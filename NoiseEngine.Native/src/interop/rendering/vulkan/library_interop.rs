use crate::{interop::prelude::{InteropResult, ResultError, ResultErrorKind}, rendering::vulkan::library};

#[no_mangle]
extern "C" fn rendering_vulkan_library_interop_create() -> InteropResult<Box<ash::Entry>> {
    match library::create() {
        Ok(library) => InteropResult::with_ok(Box::new(library)),
        Err(err) => {
            InteropResult::with_err(ResultError::with_kind(&err, ResultErrorKind::LibraryLoad))
        }
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_library_interop_destroy(_handle: Box<ash::Entry>) {
}
