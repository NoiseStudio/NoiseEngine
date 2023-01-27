using NoiseEngine.Rendering.Buffers;
using System;
using System.Reflection;

namespace NoiseEngine.Rendering;

public abstract class Mesh {

    public GraphicsDevice Device { get; }

    internal IndexFormat IndexFormat { get; }

    private protected Mesh(GraphicsDevice device, IndexFormat indexFormat) {
        Device = device;
        IndexFormat = indexFormat;
    }

    internal static IndexFormat GetIndexFormat(Type type) {
        if (type == typeof(ushort)) {
            return IndexFormat.UInt16;
        } else if (type == typeof(uint)) {
            return IndexFormat.UInt32;
        } else {
            throw new InvalidOperationException(
                $"Index type `{type.Name}` is invalid, use `ushort` or `uint` instead.");
        }
    }

    internal abstract (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) GetBuffers();

}
