#[repr(u16)]
pub(crate) enum GraphicsCommandBufferCommand {
    CopyBuffer = 0
}

impl Default for GraphicsCommandBufferCommand {
    fn default() -> Self {
        Self::CopyBuffer
    }
}
