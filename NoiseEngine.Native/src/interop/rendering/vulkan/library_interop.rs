use ash::vk;

use crate::{interop::prelude::{InteropResult, ResultError, ResultErrorKind, InteropArray}, rendering::vulkan::library};

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

#[no_mangle]
extern "C" fn rendering_vulkan_library_interop_get_extension_properties(
    library: &ash::Entry
) -> InteropResult<InteropArray<vk::ExtensionProperties>> {
    match library.enumerate_instance_extension_properties(None) {
        Ok(p) => InteropResult::with_ok(p.into()),
        Err(err) => InteropResult::with_err(err.into())
    }
}

#[no_mangle]
extern "C" fn rendering_vulkan_library_interop_get_layer_properties(
    library: &ash::Entry
) -> InteropResult<InteropArray<vk::LayerProperties>> {
    match library.enumerate_instance_layer_properties() {
        Ok(p) => InteropResult::with_ok(p.into()),
        Err(err) => InteropResult::with_err(err.into())
    }
}
