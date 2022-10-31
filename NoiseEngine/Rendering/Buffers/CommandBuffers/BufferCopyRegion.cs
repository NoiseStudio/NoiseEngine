namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal readonly record struct BufferCopyRegion(ulong SourceOffset, ulong DestinationOffset, ulong Size);
