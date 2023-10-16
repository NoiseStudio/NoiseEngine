using System;
using System.Collections.Generic;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;

namespace NoiseEngine.Tests.Rendering;

internal static class TextureTestUtils {

    public static byte[] GetColorData(
        IDictionary<Vector2<uint>, Color32> colors,
        Vector2<uint> size,
        TextureFormat format
    ) {
        if (format != TextureFormat.R8G8B8_SRGB && format != TextureFormat.R8G8B8A8_SRGB) {
            throw new ArgumentException("Invalid texture format.");
        }

        byte[] result = new byte[size.X * size.Y * (format == TextureFormat.R8G8B8_SRGB ? 3 : 4)];

        int i = 0;

        for (uint y = 0; y < size.Y; y++) {
            for (uint x = 0; x < size.X; x++) {
                result[i + 0] = colors[new Vector2<uint>(x, y)].R;
                result[i + 1] = colors[new Vector2<uint>(x, y)].G;
                result[i + 2] = colors[new Vector2<uint>(x, y)].B;
                if (format == TextureFormat.R8G8B8A8_SRGB) {
                    result[i + 3] = colors[new Vector2<uint>(x, y)].A;
                }

                i += format == TextureFormat.R8G8B8_SRGB ? 3 : 4;
            }
        }

        return result;
    }

    public static bool CompareLossy(
        ReadOnlySpan<byte> data,
        Vector2<uint> size,
        IDictionary<Vector2<uint>, Color32> expectedColors,
        TextureFormat format,
        int maxDifference = 10
    ) {
        if (format != TextureFormat.R8G8B8A8_SRGB)
            throw new ArgumentException("Unsupported texture format.");

        for (int y = 0; y < size.Y; y++) {
            for (int x = 0; x < size.X; x++) {
                Color32 expectedColor = expectedColors[new Vector2<uint>((uint)x, (uint)y)];
                Color32 actualColor = new Color32(
                    data[(y * (int)size.X + x) * 4 + 0],
                    data[(y * (int)size.X + x) * 4 + 1],
                    data[(y * (int)size.X + x) * 4 + 2],
                    data[(y * (int)size.X + x) * 4 + 3]
                );

                int difference =
                    Math.Abs(expectedColor.R - actualColor.R) +
                    Math.Abs(expectedColor.G - actualColor.G) +
                    Math.Abs(expectedColor.B - actualColor.B) +
                    Math.Abs(expectedColor.A - actualColor.A);

                if (difference > maxDifference)
                    return false;
            }

        }

        return true;
    }

    public static bool CompareLossy(Texture2D expected, Texture2D actual, float maxDifference = 0.01f) {
        if (expected.Format != actual.Format)
            throw new ArgumentException("Texture formats do not match.");
        if (expected.Width != actual.Width || expected.Height != actual.Height)
            throw new ArgumentException("Texture sizes do not match.");
        if (expected.Format != TextureFormat.R8G8B8A8_SRGB && expected.Format != TextureFormat.R8G8B8A8_UNORM)
            throw new ArgumentException("Unsupported texture format.");

        Color32[] expectedData = new Color32[expected.Width * expected.Height];
        expected.GetPixels<Color32>(expectedData);
        Color32[] actualData = new Color32[actual.Width * actual.Height];
        actual.GetPixels<Color32>(actualData);

        for (int i = 0; i < expectedData.Length; i++) {
            Color e = expectedData[i];
            Color a = actualData[i];

            if (
                MathF.Abs(e.R - a.R) > maxDifference ||
                MathF.Abs(e.G - a.G) > maxDifference ||
                MathF.Abs(e.B - a.B) > maxDifference ||
                MathF.Abs(e.A - a.A) > maxDifference
            ) {
                throw new Exception($"Expected {e}, got {a}. {i}");
                return false;
            }
        }

        return true;
    }

}
