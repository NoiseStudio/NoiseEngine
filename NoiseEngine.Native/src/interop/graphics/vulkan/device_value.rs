use uuid::Uuid;

use crate::{
    graphics::{vulkan::device::VulkanDevice},
    interop::prelude::InteropString
};

#[repr(C)]
pub(super) struct VulkanDeviceValue {
    pub name: InteropString,
    pub vendor: u32,
    pub device_type: u32,
    pub api_version: u32,
    pub driver_version: u32,
    pub guid: Uuid,
    pub is_supports_graphics: bool,
    pub is_supports_computing: bool,
    pub handle: Box<VulkanDevice>
}
