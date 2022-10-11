use std::sync::Arc;

use vulkano::{VulkanLibrary, LoadingError::{LibraryLoadFailure, OomError}};

use crate::{interop::prelude::{InteropResult, ResultError, ResultErrorKind}, graphics::vulkan::library};

#[no_mangle]
extern "C" fn graphics_vulkan_library_interop_create() -> InteropResult<Box<Arc<VulkanLibrary>>> {
    match library::create() {
        Ok(library) => InteropResult::with_ok(Box::new(library)),
        Err(err) => { 
            InteropResult::with_err(match err {
                LibraryLoadFailure(load) => {
                    ResultError::with_kind(&load, ResultErrorKind::LibraryLoad)
                },
                OomError(oom) => ResultError::with_kind(&err, oom.into())
            })
        }
    }
}

#[no_mangle]
extern "C" fn graphics_vulkan_library_interop_destroy(_handle: Box<Arc<VulkanLibrary>>) {
}
