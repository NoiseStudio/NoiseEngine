use noise_engine_native::{interop::prelude::InteropResult, errors::overflow::OverflowError};
use vulkano::Version;

#[no_mangle]
extern "C" fn interop_graphics_vulkan_version_test_managed_create(version: u32) -> InteropResult<u32> {
    match u32::try_from(Version::from(version)) {
        Ok(v) => InteropResult::with_ok(v),
        Err(_) => InteropResult::with_err(OverflowError::default().into())
    }
}

#[no_mangle]
extern "C" fn interop_graphics_vulkan_version_test_unmanaged_create(
    major: i32, minor: i32, revision: i32
) -> InteropResult<u32> {
    match u32::try_from(Version {
        major: major as u32,
        minor: minor as u32,
        patch: revision as u32
    }) {
        Ok(v) => InteropResult::with_ok(v),
        Err(_) => InteropResult::with_err(OverflowError::default().into())
    }
}
