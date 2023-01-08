#[repr(u16)]
pub enum GraphicsCommandBufferCommand {
    CopyBuffer = 0,
    CopyTextureToBuffer = 1,
    Dispatch = 2,
    AttachCamera = 3,
    DetachCamera = 4
}
