use std::{ptr, ffi::{CStr, CString}};

use ash::vk;

use crate::interop::prelude::InteropString;

#[repr(C)]
pub(crate) struct VulkanApplicationInfo {
    pub application_name: InteropString,
    pub application_version: u32,
    pub engine_version: u32
}

impl From<VulkanApplicationInfo> for vk::ApplicationInfo {
    fn from(create_info: VulkanApplicationInfo) -> Self {
        let application_name = CString::new(String::from(create_info.application_name)).unwrap();
        let engine_name = unsafe {
            CStr::from_bytes_with_nul_unchecked(b"NoiseEngine\0")
        };

        vk::ApplicationInfo {
            s_type: vk::StructureType::APPLICATION_INFO,
            p_next: ptr::null(),
            p_application_name: application_name.as_ptr(),
            application_version: create_info.application_version,
            p_engine_name: engine_name.as_ptr(),
            engine_version: create_info.engine_version,
            api_version: vk::make_api_version(0, 1, 3, 0)
        }
    }
}
