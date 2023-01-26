using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public class Mesh<TVertex, TIndex> : Mesh where TVertex : unmanaged where TIndex : unmanaged {

    private readonly GraphicsDeviceBuffer<TVertex> vertexBuffer;
    private readonly GraphicsDeviceBuffer<TIndex> indexBuffer;

    private IndexFormat IndexFormat { get; }

    public Mesh(GraphicsDevice device, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<TIndex> indices) : base(device) {
        if (typeof(TIndex) == typeof(ushort)) {
            IndexFormat = IndexFormat.UInt16;
        } else if (typeof(TIndex) == typeof(uint)) {
            IndexFormat = IndexFormat.UInt32;
        } else {
            throw new InvalidOperationException(
                $"Index type `{typeof(TIndex).Name}` is invalid, use `ushort` or `uint` instead.");
        }

        vertexBuffer = new GraphicsDeviceBuffer<TVertex>(device, GraphicsBufferUsage.Vertex, vertices);
        indexBuffer = new GraphicsDeviceBuffer<TIndex>(device, GraphicsBufferUsage.Index, indices);
    }

    internal override (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) GetBuffers() {
        return (vertexBuffer, indexBuffer);
    }

}
