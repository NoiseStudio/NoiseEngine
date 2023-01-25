use ash::vk;

#[repr(C)]
pub struct GraphicsPipelineCreateInfo {
    pub primitive_topology: vk::PrimitiveTopology
}
