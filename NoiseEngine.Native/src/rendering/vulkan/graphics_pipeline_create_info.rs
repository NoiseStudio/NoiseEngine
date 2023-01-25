use std::marker::PhantomData;

#[repr(C)]
pub struct GraphicsPipelineCreateInfo {
    phantom: PhantomData<()>,
}
