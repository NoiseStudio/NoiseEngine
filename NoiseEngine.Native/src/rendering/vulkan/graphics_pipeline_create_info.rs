use ash::vk;

use crate::interop::interop_read_only_span::InteropReadOnlySpan;

#[repr(C)]
pub struct GraphicsPipelineCreateInfo<'a> {
    pub vertex_input_binding_descriptions: InteropReadOnlySpan<'a, vk::VertexInputBindingDescription>,
    pub vertex_input_attribute_descriptions: InteropReadOnlySpan<'a, vk::VertexInputAttributeDescription>,
    pub primitive_topology: vk::PrimitiveTopology
}
