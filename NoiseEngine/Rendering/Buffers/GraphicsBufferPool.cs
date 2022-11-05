namespace NoiseEngine.Rendering.Buffers;

// TODO: Implement real pool. https://github.com/NoiseStudio/NoiseEngine/issues/158
internal class GraphicsBufferPool {

    public GraphicsDevice Device { get; }

    public GraphicsBufferPool(GraphicsDevice device) {
        Device = device;
    }

    public GraphicsHostBuffer<T> GetOrCreateHost<T>(GraphicsBufferUsage usage, ulong count) where T : unmanaged {
        return new GraphicsHostBuffer<T>(Device, usage, count);
    }

    public void UnsafeReturnHostToPool<T>(GraphicsHostBuffer<T> buffer) where T : unmanaged {
        // TODO: Implement return.
    }

}
