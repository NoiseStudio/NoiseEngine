namespace NoiseEngine.Rendering.Buffers;

public enum GraphicsBufferUsage : uint {
    TransferSource = 1 << 0,
    TransferDestination = 1 << 1,
    UniformTexel = 1 << 2,
    StorageTexel = 1 << 3,
    Uniform = 1 << 4,
    Storage = 1 << 5,
    Index = 1 << 6,
    Vertex = 1 << 7,
    Indirect = 1 << 8,

    TransferAll = TransferSource | TransferDestination
}
