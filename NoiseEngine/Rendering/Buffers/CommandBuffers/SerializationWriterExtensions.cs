using NoiseEngine.Serialization;

namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal static class SerializationWriterExtensions {

    public static void WriteCommand(this SerializationWriter writer, CommandBufferCommand command) {
        writer.WriteUInt16((ushort)command);
    }

}
