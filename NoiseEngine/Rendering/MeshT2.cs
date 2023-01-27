using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public class Mesh<TVertex, TIndex> : Mesh where TVertex : unmanaged where TIndex : unmanaged {

    private readonly GraphicsDeviceBuffer<TVertex> vertexBuffer;
    private readonly GraphicsDeviceBuffer<TIndex> indexBuffer;

    public Mesh(
        GraphicsDevice device, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<TIndex> indices
    ) : base(device, GetIndexFormat(typeof(TIndex))) {
        vertexBuffer = new GraphicsDeviceBuffer<TVertex>(device, GraphicsBufferUsage.Vertex, vertices);
        indexBuffer = new GraphicsDeviceBuffer<TIndex>(device, GraphicsBufferUsage.Index, indices);
    }

    internal override (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) GetBuffers() {
        return (vertexBuffer, indexBuffer);
    }

}
