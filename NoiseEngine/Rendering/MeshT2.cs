using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Buffers.CommandBuffers;
using System;

namespace NoiseEngine.Rendering;

public class Mesh<TVertex, TIndex> : Mesh where TVertex : unmanaged where TIndex : unmanaged {

    private readonly GraphicsDeviceBuffer<TVertex> vertexBuffer;
    private readonly GraphicsDeviceBuffer<TIndex> indexBuffer;

    public Mesh(
        GraphicsDevice device, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<TIndex> indices
    ) : base(device, GetIndexFormat(typeof(TIndex))) {
        ulong verticesCount = (ulong)vertices.Length;
        ulong indicesCount = (ulong)indices.Length;

        vertexBuffer = new GraphicsDeviceBuffer<TVertex>(device, GraphicsBufferUsage.Vertex, verticesCount);
        indexBuffer = new GraphicsDeviceBuffer<TIndex>(device, GraphicsBufferUsage.Index, indicesCount);

        GraphicsHostBuffer<TVertex> vertexHost = Device.BufferPool.GetOrCreateHost<TVertex>(
            GraphicsBufferUsage.TransferAll, verticesCount
        );
        vertexHost.SetData(vertices);

        GraphicsHostBuffer<TIndex> indexHost = Device.BufferPool.GetOrCreateHost<TIndex>(
            GraphicsBufferUsage.TransferAll, indicesCount
        );
        indexHost.SetData(indices);

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(Device, false);

        commandBuffer.CopyUnchecked(vertexHost, vertexBuffer, stackalloc BufferCopyRegion[] {
            new BufferCopyRegion(0, 0, GraphicsReadOnlyBuffer<TVertex>.GetSize(verticesCount))
        });
        commandBuffer.CopyUnchecked(indexHost, indexBuffer, stackalloc BufferCopyRegion[] {
            new BufferCopyRegion(0, 0, GraphicsReadOnlyBuffer<TIndex>.GetSize(indicesCount))
        });

        commandBuffer.Execute();
        commandBuffer.Clear();

        Device.BufferPool.UnsafeReturnHostToPool(vertexHost);
        Device.BufferPool.UnsafeReturnHostToPool(indexHost);
    }

    internal override (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) GetBuffers() {
        return (vertexBuffer, indexBuffer);
    }

}
