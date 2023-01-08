using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color(float R, float G, float B, float A) {

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

    public Color(float r, float g, float b) : this(r, g, b, 1) {
    }

}
