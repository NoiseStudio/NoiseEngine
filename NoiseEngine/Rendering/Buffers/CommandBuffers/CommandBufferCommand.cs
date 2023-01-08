namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal enum CommandBufferCommand : ushort {
    CopyBuffer = 0,
    Dispatch = 1,
    AttachCamera = 2,
    DetachCamera = 3
}
