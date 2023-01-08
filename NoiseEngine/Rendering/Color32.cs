using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color32(byte R, byte G, byte B, byte A) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ConvertColorFloatToByte(float f) {
        return (byte)Math.Max(0, Math.Min(255, (int)(f * 255f)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Color32(Color color) {
        return new Color32(
            ConvertColorFloatToByte(color.R),
            ConvertColorFloatToByte(color.G),
            ConvertColorFloatToByte(color.B),
            ConvertColorFloatToByte(color.A)
        );
    }

}
