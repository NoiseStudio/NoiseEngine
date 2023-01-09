#[repr(u16)]
pub enum GraphicsCommandBufferCommand {
    CopyBuffer = 0,
    CopyBufferToTexture = 1,
    CopyTextureToBuffer = 2,
    Dispatch = 3,
    AttachCamera = 4,
    DetachCamera = 5
}
