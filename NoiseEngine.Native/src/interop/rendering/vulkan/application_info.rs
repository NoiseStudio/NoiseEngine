use std::{ptr, ffi::{CStr, CString}};

use ash::vk;

use crate::{
    interop::prelude::InteropString, rendering::vulkan::errors::universal::VulkanUniversalError,
    errors::invalid_operation::InvalidOperationError
};

#[repr(C)]
pub(crate) struct VulkanApplicationInfo {
    pub application_name: InteropString,
    pub application_version: u32,
    pub engine_version: u32
}

impl From<VulkanApplicationInfo> for Result<vk::ApplicationInfo, VulkanUniversalError> {
    fn from(create_info: VulkanApplicationInfo) -> Self {
        let application_name = match CString::new(String::from(create_info.application_name)) {
            Ok(s) => s,
            Err(_) => return Err(
                InvalidOperationError::with_str("Application name contains null character.").into()
            )
        };

        let engine_name = unsafe {
            CStr::from_bytes_with_nul_unchecked(b"NoiseEngine\0")
        };

        Ok(vk::ApplicationInfo {
            s_type: vk::StructureType::APPLICATION_INFO,
            p_next: ptr::null(),
            p_application_name: application_name.as_ptr(),
            application_version: create_info.application_version,
            p_engine_name: engine_name.as_ptr(),
            engine_version: create_info.engine_version,
            api_version: vk::make_api_version(0, 1, 3, 0)
        })
    }
}
