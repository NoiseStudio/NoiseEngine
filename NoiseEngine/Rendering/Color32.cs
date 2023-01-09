using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color32(byte R, byte G, byte B, byte A) {

    public static Color Black => new Color(0.0f, 0.0f, 0.0f);
    public static Color Gray => new Color(0.5f, 0.5f, 0.5f);
    public static Color White => new Color(1.0f, 1.0f, 1.0f);
    public static Color Transparent => new Color(0.0f, 0.0f, 0.0f, 0.0f);

    public static Color Red => new Color(1.0f, 0.0f, 0.0f);
    public static Color Green => new Color(0.0f, 1.0f, 0.0f);
    public static Color Blue => new Color(0.0f, 0.0f, 1.0f);

    public static Color Yellow => new Color(1.0f, 1.0f, 0.0f);
    public static Color Cyan => new Color(0.0f, 1.0f, 1.0f);
    public static Color Magenta => new Color(1.0f, 0.0f, 1.0f);

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
