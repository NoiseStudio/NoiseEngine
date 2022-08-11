using NoiseEngine.Collections;
using System;
using System.Buffers.Binary;
using System.Text;

namespace NoiseEngine.Nesl.Emit.Attributes;

internal static class AttributeHelper {

    public static void WriteBytes(FastList<byte> buffer, byte[]? array) {
        Span<byte> span = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(span, array?.Length ?? -1);

        buffer.AddRange(span);

        if (array is not null)
            buffer.AddRange(array);
    }

    public static ReadOnlySpan<byte> ReadBytes(ReadOnlySpan<byte> span) {
        int length = BinaryPrimitives.ReadInt32LittleEndian(span);

        if (length == -1)
            return null;
        return span.Slice(sizeof(int), length);
    }

    public static void WriteString(FastList<byte> buffer, string? str) {
        WriteBytes(buffer, str is not null ? Encoding.UTF8.GetBytes(str) : null);
    }

    public static string? ReadString(ReadOnlySpan<byte> span) {
        ReadOnlySpan<byte> stringSpan = ReadBytes(span);
        return stringSpan != null ? Encoding.UTF8.GetString(stringSpan) : null;
    }

    public static ReadOnlySpan<byte> JumpToNextBytes(ReadOnlySpan<byte> span) {
        int length = BinaryPrimitives.ReadInt32LittleEndian(span);

        int start = sizeof(int);
        if (length > 0)
            start += length;

        return span.Slice(start);
    }

}
