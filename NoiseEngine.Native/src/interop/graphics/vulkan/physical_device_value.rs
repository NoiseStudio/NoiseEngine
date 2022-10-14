use std::sync::Arc;

use uuid::Uuid;
use vulkano::device::physical::PhysicalDevice;

use crate::interop::{prelude::InteropString, graphics::physical_device_type::GraphicsPhysicalDeviceType};

#[repr(C)]
pub(super) struct VulkanPhysicalDeviceValue {
    pub name: InteropString,
    pub vendor: u32,
    pub device_type: GraphicsPhysicalDeviceType,
    pub api_version: u32,
    pub driver_version: u32,
    pub guid: Uuid,
    pub handle: Box<Arc<PhysicalDevice>>
}
