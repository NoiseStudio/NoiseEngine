using System;
using System.Collections.Generic;
using System.IO;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class CpuTexture2DTest : GraphicsTestEnvironment {

    public CpuTexture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    private readonly Dictionary<Vector2<uint>, Color32> textureColors = new Dictionary<Vector2<uint>, Color32> {
        { new Vector2<uint>(0, 0), new Color32(255, 0, 0) },
        { new Vector2<uint>(1, 0), new Color32(0, 255, 0) },
        { new Vector2<uint>(2, 0), new Color32(0, 0, 0) },
        { new Vector2<uint>(0, 1), new Color32(0, 0, 255) },
        { new Vector2<uint>(1, 1), new Color32(255, 255, 0) },
        { new Vector2<uint>(2, 1), new Color32(255, 0, 255) }
    };

    [Theory]
    [InlineData(TextureFileFormat.Png, "colors.png", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData(TextureFileFormat.Jpeg, "colors.jpeg", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData(TextureFileFormat.Webp, "colors.webp", TextureFormat.R8G8B8A8_SRGB)]
    public void FromFile(TextureFileFormat fileFormat, string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, fileFormat);

        Assert.Equal<uint>(3, texture.Width);
        Assert.Equal<uint>(2, texture.Height);
        Assert.Equal(format, texture.Format);

        if (fileFormat == TextureFileFormat.Jpeg) {
            // Compression moment
            return;
        }

        byte[] expected = GetColorData(textureColors, new Vector2<uint>(texture.Width, texture.Height), format);
        Assert.Equal(expected, texture.Data.ToArray());
    }

    [Theory]
    [InlineData(TextureFileFormat.Png, "colors.png", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData(TextureFileFormat.Jpeg, "colors.jpeg", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData(TextureFileFormat.Webp, "colors.webp", TextureFormat.R8G8B8A8_SRGB)]
    public void ToTexture2D(TextureFileFormat fileFormat, string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, fileFormat);

        byte[] expected = GetColorData(textureColors, new Vector2<uint>(texture.Width, texture.Height), format);

        foreach (GraphicsDevice device in GraphicsDevices) {
            Texture2D texture2D = texture.ToTexture2D(device, TextureUsage.TransferAll);
            Assert.Equal(format, texture2D.Format);
            Assert.Equal<uint>(3, texture2D.Width);
            Assert.Equal<uint>(2, texture2D.Height);
            byte[] actual = new byte[expected.Length];

            if (fileFormat == TextureFileFormat.Jpeg) {
                // Compression moment
                continue;
            }

            texture2D.GetPixels(actual.AsSpan());
            Assert.Equal(expected, actual);
        }
    }

    private static byte[] GetColorData(
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

}
