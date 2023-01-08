namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal enum CommandBufferCommand : ushort {
    CopyBuffer = 0,
    CopyTextureToBuffer = 1,
    Dispatch = 2,
    AttachCamera = 3,
    DetachCamera = 4
}
