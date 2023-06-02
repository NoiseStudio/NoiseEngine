using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoiseEngine.Interop;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class CpuTexture2DTest : GraphicsTestEnvironment {

    private readonly Dictionary<Vector2<uint>, Color32> textureColors = new Dictionary<Vector2<uint>, Color32> {
        { new Vector2<uint>(0, 0), new Color32(255, 0, 0) },
        { new Vector2<uint>(1, 0), new Color32(0, 255, 0) },
        { new Vector2<uint>(2, 0), new Color32(0, 0, 0) },
        { new Vector2<uint>(0, 1), new Color32(0, 0, 255) },
        { new Vector2<uint>(1, 1), new Color32(255, 255, 0) },
        { new Vector2<uint>(2, 1), new Color32(255, 0, 255) }
    };

    public CpuTexture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Theory]
    [InlineData("colors.png", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.webp", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.jpeg", TextureFormat.R8G8B8A8_SRGB)]
    public void FromFile(string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, format);

        Assert.Equal<uint>(3, texture.Width);
        Assert.Equal<uint>(2, texture.Height);
        Assert.Equal(format, texture.Format);

        byte[] expected = TextureTestUtils.GetColorData(
            textureColors,
            new Vector2<uint>(texture.Width, texture.Height),
            format);

        if (path.EndsWith(".jpg") || path.EndsWith(".jpeg")) {
            Assert.True(
                TextureTestUtils.CompareLossy(
                    texture.Data,
                    new Vector2<uint>(texture.Width, texture.Height),
                    textureColors,
                    format
                )
            );
        } else {
            Assert.Equal(expected, texture.Data.ToArray());
        }


    }

    [Theory]
    [InlineData("colors.png", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.webp", TextureFormat.R8G8B8A8_SRGB)]
    [InlineData("colors.jpeg", TextureFormat.R8G8B8A8_SRGB)]
    public void ToTexture2D(string path, TextureFormat format) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, format);

        byte[] expected = TextureTestUtils.GetColorData(
            textureColors,
            new Vector2<uint>(texture.Width, texture.Height),
            format);

        foreach (GraphicsDevice device in GraphicsDevices) {
            Texture2D texture2D = texture.ToTexture2D(device, TextureUsage.TransferAll);
            Assert.Equal(format, texture2D.Format);
            Assert.Equal<uint>(3, texture2D.Width);
            Assert.Equal<uint>(2, texture2D.Height);
            byte[] actual = new byte[expected.Length];
            texture2D.GetPixels(actual.AsSpan());

            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg")) {
                Assert.True(
                    TextureTestUtils.CompareLossy(
                        actual,
                        new Vector2<uint>(texture.Width, texture.Height),
                        textureColors,
                        format
                    )
                );
            } else {
                Assert.Equal(expected, actual);
            }
        }
    }

    [Theory]
    [InlineData("colors.png", TextureFileFormat.Png, null)]
    [InlineData("colors.jpeg", TextureFileFormat.Jpeg, (byte)100)]
    [InlineData("colors.webp", TextureFileFormat.Webp, null)]
    public void ToFile(string path, TextureFileFormat fileFormat, byte? quality) {
        byte[] fileData = File.ReadAllBytes($"./Resources/Textures/{path}");
        CpuTexture2D expected = CpuTexture2D.FromFile(fileData, TextureFormat.R8G8B8A8_SRGB);
        CpuTexture2D actual = CpuTexture2D.FromFile(expected.ToFile(fileFormat, quality), TextureFormat.R8G8B8A8_SRGB);
        
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        
        if (path.EndsWith(".jpg") || path.EndsWith(".jpeg")) {
            Assert.True(
                TextureTestUtils.CompareLossy(
                    actual.Data.ToArray(),
                    new Vector2<uint>(actual.Width, actual.Height),
                    textureColors,
                    TextureFormat.R8G8B8A8_SRGB
                )
            );
        } else {
            Assert.Equal(expected.Data.ToArray(), actual.Data.ToArray());
        }
    }

}
