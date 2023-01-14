#[repr(u16)]
pub enum GraphicsCommandBufferCommand {
    CopyBuffer = 0,
    CopyBufferToTexture = 1,
    CopyTextureToBuffer = 2,
    Dispatch = 3,
    AttachCameraWindow = 4,
    AttachCameraTexture = 5,
    DetachCamera = 6
}
