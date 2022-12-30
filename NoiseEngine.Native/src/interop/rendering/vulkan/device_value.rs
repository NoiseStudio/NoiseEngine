use std::sync::Arc;

use uuid::Uuid;

use crate::{interop::prelude::InteropString, rendering::vulkan::device::VulkanDevice};

#[repr(C)]
pub(crate) struct VulkanDeviceValue<'init> {
    pub name: InteropString,
    pub vendor: u32,
    pub device_type: u32,
    pub api_version: u32,
    pub driver_version: u32,
    pub guid: Uuid,
    pub supports_graphics: bool,
    pub supports_computing: bool,
    pub handle: Box<Arc<VulkanDevice<'init>>>
}
