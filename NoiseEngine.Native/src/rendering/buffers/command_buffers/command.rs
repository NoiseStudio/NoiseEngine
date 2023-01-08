#[repr(u16)]
pub enum GraphicsCommandBufferCommand {
    CopyBuffer = 0,
    Dispatch = 1,
    AttachCamera = 2,
    DetachCamera = 3
}
