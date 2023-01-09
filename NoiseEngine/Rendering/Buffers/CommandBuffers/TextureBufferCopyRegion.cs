using NoiseEngine.Mathematics;
using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal readonly record struct TextureBufferCopyRegion(
    ulong BufferOffset, Vector3<int> ImageOffset, Vector3<uint> ImageSize, TextureAspect Aspect, uint MipLevel,
    uint LayerStartIndex, uint LayerCount
) {

    public void Write(SerializationWriter writer) {
        writer.WriteUInt64(BufferOffset);

        writer.WriteUInt32((uint)Aspect);
        writer.WriteUInt32(MipLevel);
        writer.WriteUInt32(LayerStartIndex);
        writer.WriteUInt32(LayerCount);

        writer.WriteInt32(ImageOffset.X);
        writer.WriteInt32(ImageOffset.Y);
        writer.WriteInt32(ImageOffset.Z);

        writer.WriteUInt32(ImageSize.X);
        writer.WriteUInt32(ImageSize.Y);
        writer.WriteUInt32(ImageSize.Z);
    }

}
