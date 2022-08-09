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
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(), value);

        return new SpirVLiteral(result);
    }

}
