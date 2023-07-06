use std::collections::HashMap;

use ash::vk::DescriptorType;

pub struct DescriptorPoolSizes {
    pub map: HashMap<DescriptorType, u32>,
    pub count: u32,
}
