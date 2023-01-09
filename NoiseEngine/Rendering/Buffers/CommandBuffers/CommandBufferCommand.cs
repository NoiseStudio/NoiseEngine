namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal enum CommandBufferCommand : ushort {
    CopyBuffer = 0,
    CopyBufferToTexture = 1,
    CopyTextureToBuffer = 2,
    Dispatch = 3,
    AttachCamera = 4,
    DetachCamera = 5
}
