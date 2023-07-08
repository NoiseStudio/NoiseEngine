using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal static class SpirVLiteralExtensions {

    public static SpirVLiteral ToSpirVLiteral(this string str) {
        List<byte> result = new List<byte>(Encoding.UTF8.GetBytes(str));

        result.Add(0); // null termination

        int count = (sizeof(uint) - result.Count % sizeof(uint)) % sizeof(uint);
        for (int i = 0; i < count; i++)
            result.Add(0);

        return new SpirVLiteral(result);
    }

    public static SpirVLiteral ToSpirVLiteral(this uint value) {
        byte[] result = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(), value);

        return new SpirVLiteral(result);
    }

    public static SpirVLiteral ToSpirVLiteral(this int value) {
        byte[] result = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(), value);

        return new SpirVLiteral(result);
    }

    public static SpirVLiteral ToSpirVLiteral(this long value) {
        if (value >= int.MinValue && value <= int.MaxValue)
            return ((int)value).ToSpirVLiteral();

        byte[] result = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(), value);

        return new SpirVLiteral(result);
    }

    public static SpirVLiteral ToSpirVLiteral(this float value) {
        byte[] result = new byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(), value);

        return new SpirVLiteral(result);
    }

}
