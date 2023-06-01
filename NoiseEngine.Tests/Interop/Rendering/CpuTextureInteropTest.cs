using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;
using NoiseEngine.Tests.Rendering;

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
    [InlineData("colors.png", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.webp", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.jpeg", TextureFormat.R8G8B8A8_SRGB)]
    public void Decode(string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, format);

        Assert.True(result.IsOk);
        CpuTextureData data = result.Value;

        Assert.Equal<uint>(3, data.ExtentX);
        Assert.Equal<uint>(2, data.ExtentY);
        Assert.Equal<uint>(1, data.ExtentZ);
        Assert.Equal(format, data.Format);

        byte[] expected = TextureTestUtils.GetColorData(
            textureColors,
            new Vector2<uint>(data.ExtentX, data.ExtentY),
            format);

        if (!path.EndsWith(".jpg") && !path.EndsWith(".jpeg")) {
            Assert.Equal(expected, data.Data.ToArray());
            return;
        }

        for (int y = 0; y < data.ExtentY; y++) {
            for (int x = 0; x < data.ExtentX; x++) {
                Color32 expectedColor = textureColors[new Vector2<uint>((uint)x, (uint)y)];
                Color32 actualColor = new Color32(
                    data.Data[(y * (int)data.ExtentX + x) * 4 + 0],
                    data.Data[(y * (int)data.ExtentX + x) * 4 + 1],
                    data.Data[(y * (int)data.ExtentX + x) * 4 + 2],
                    data.Data[(y * (int)data.ExtentX + x) * 4 + 3]
                );

                int difference =
                    Math.Abs(expectedColor.R - actualColor.R) +
                    Math.Abs(expectedColor.G - actualColor.G) +
                    Math.Abs(expectedColor.B - actualColor.B) +
                    Math.Abs(expectedColor.A - actualColor.A);

                Assert.True(difference < 10);
            }
        }
    }

    [Fact]
    public void NotDecode() {
        byte[] fileData = new byte[256];
        Random.Shared.NextBytes(fileData);
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, TextureFormat.R8G8B8A8_SRGB);
        Assert.False(result.IsOk);
    }

}
