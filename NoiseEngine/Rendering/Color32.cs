using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color32(byte R, byte G, byte B, byte A) {

    public static Color32 Black => new Color32(0, 0, 0);
    public static Color32 Gray => new Color32(0x80, 0x80, 0x80);
    public static Color32 White => new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static Color32 Transparent => new Color32(0, 0, 0, 0);

    public static Color32 Red => new Color32(byte.MaxValue, 0, 0);
    public static Color32 Green => new Color32(0, byte.MaxValue, 0);
    public static Color32 Blue => new Color32(0, 0, byte.MaxValue);

    public static Color32 Yellow => new Color32(byte.MaxValue, byte.MaxValue, 0);
    public static Color32 Cyan => new Color32(0, byte.MaxValue, byte.MaxValue);
    public static Color32 Magenta => new Color32(byte.MaxValue, 0, byte.MaxValue);

    public Color32(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue) {
    }

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
