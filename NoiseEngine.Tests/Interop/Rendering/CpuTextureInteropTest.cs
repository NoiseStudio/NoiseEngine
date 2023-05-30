using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;

namespace NoiseEngine.Tests.Interop.Rendering;

public class CpuTextureInteropTest {

    private readonly Dictionary<Vector2<uint>, Color32> textureColors = new Dictionary<Vector2<uint>, Color32> {
        { new Vector2<uint>(0, 0), new Color32(255, 0, 0) },
        { new Vector2<uint>(1, 0), new Color32(0, 255, 0) },
        { new Vector2<uint>(2, 0), new Color32(0, 0, 0) },
        { new Vector2<uint>(0, 1), new Color32(0, 0, 255) },
        { new Vector2<uint>(1, 1), new Color32(255, 255, 0) },
        { new Vector2<uint>(2, 1), new Color32(255, 0, 255) }
    };

    [Theory]
    [InlineData(TextureFileFormat.Png, "colors.png", TextureFormat.R8G8B8_UINT)]
    [InlineData(TextureFileFormat.Jpeg, "colors.jpeg", TextureFormat.R8G8B8_UINT)]
    [InlineData(TextureFileFormat.Webp, "colors.webp", TextureFormat.R8G8B8A8_UINT)]
    public void Decode(TextureFileFormat fileFormat, string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, fileFormat);

        Assert.True(result.IsOk);
        CpuTextureData data = result.Value;

        Assert.Equal<uint>(3, data.ExtentX);
        Assert.Equal<uint>(2, data.ExtentY);
        Assert.Equal<uint>(1, data.ExtentZ);
        Assert.Equal(format, data.Format);

        if (fileFormat == TextureFileFormat.Jpeg) {
            // Compression moment
            return;
        }

        byte[] expected = GetColorData(textureColors, new Vector2<uint>(data.ExtentX, data.ExtentY), format);
        Assert.Equal(expected, data.Data.ToArray());
    }

    [Theory]
    [InlineData(TextureFileFormat.Png)]
    [InlineData(TextureFileFormat.Jpeg)]
    [InlineData(TextureFileFormat.Webp)]
    public void NotDecode(TextureFileFormat format) {
        byte[] fileData = new byte[256];
        Random.Shared.NextBytes(fileData);
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, format);
        Assert.False(result.IsOk);
    }

    private static byte[] GetColorData(
        IDictionary<Vector2<uint>, Color32> colors,
        Vector2<uint> size,
        TextureFormat format
    ) {
        if (format != TextureFormat.R8G8B8_UINT && format != TextureFormat.R8G8B8A8_UINT) {
            throw new ArgumentException("Invalid texture format.");
        }

        byte[] result = new byte[size.X * size.Y * (format == TextureFormat.R8G8B8_UINT ? 3 : 4)];

        int i = 0;

        for (uint y = 0; y < size.Y; y++) {
            for (uint x = 0; x < size.X; x++) {
                result[i + 0] = colors[new Vector2<uint>(x, y)].R;
                result[i + 1] = colors[new Vector2<uint>(x, y)].G;
                result[i + 2] = colors[new Vector2<uint>(x, y)].B;
                if (format == TextureFormat.R8G8B8A8_UINT) {
                    result[i + 3] = colors[new Vector2<uint>(x, y)].A;
                }

                i += format == TextureFormat.R8G8B8_UINT ? 3 : 4;
            }
        }

        return result;
    }

}
