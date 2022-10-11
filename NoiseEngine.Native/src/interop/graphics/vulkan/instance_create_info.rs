use vulkano::instance::InstanceCreateInfo;

use crate::interop::prelude::InteropString;

#[repr(C)]
pub(crate) struct VulkanInstanceCreateInfo {
    pub application_name: InteropString,
    pub application_version: u32,
    pub engine_version: u32
}

impl From<VulkanInstanceCreateInfo> for InstanceCreateInfo {
    fn from(create_info: VulkanInstanceCreateInfo) -> Self {
        InstanceCreateInfo {
            application_name: Some(String::from(create_info.application_name)),
            application_version: create_info.application_version.into(),
            engine_name: Some("NoiseEngine".to_string()),
            engine_version: create_info.engine_version.into(),
            enumerate_portability: true,
            ..Default::default()
        }
    }
}
