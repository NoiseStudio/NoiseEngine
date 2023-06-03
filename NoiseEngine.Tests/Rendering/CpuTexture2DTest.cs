using System;
using System.Collections.Generic;
using System.IO;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

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

    [Fact]
    public void FromTexture2D() {
        byte[] fileData = File.ReadAllBytes("./Resources/Textures/colors.png");
        CpuTexture2D expected = CpuTexture2D.FromFile(fileData, TextureFormat.R8G8B8A8_SRGB);

        foreach (GraphicsDevice device in GraphicsDevices) {
            Texture2D texture2D = expected.ToTexture2D(device, TextureUsage.TransferAll);
            CpuTexture2D actual = CpuTexture2D.FromTexture2D(texture2D);

            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            Assert.Equal(expected.Format, actual.Format);
            Assert.Equal(expected.Data.ToArray(), actual.Data.ToArray());
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

    [Fact]
    public void ToPng() {
        byte[] fileData = File.ReadAllBytes("./Resources/Textures/colors.png");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, TextureFormat.R8G8B8A8_SRGB);

        byte[] expected = texture.Data.ToArray();
        byte[] actual = CpuTexture2D.FromFile(texture.ToPng()).Data.ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToJpeg() {
        byte[] fileData = File.ReadAllBytes("./Resources/Textures/colors.jpeg");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, TextureFormat.R8G8B8A8_SRGB);

        Span<byte> actual = CpuTexture2D.FromFile(texture.ToJpeg()).Data;

        TextureTestUtils.CompareLossy(
            actual,
            new Vector2<uint>(texture.Width, texture.Height),
            textureColors,
            texture.Format
        );
    }

    [Fact]
    public void ToWebp() {
        byte[] fileData = File.ReadAllBytes("./Resources/Textures/colors.webp");
        CpuTexture2D texture = CpuTexture2D.FromFile(fileData, TextureFormat.R8G8B8A8_SRGB);

        byte[] expected = texture.Data.ToArray();
        byte[] actual = CpuTexture2D.FromFile(texture.ToWebP(), TextureFormat.R8G8B8A8_SRGB).Data.ToArray();

        Assert.Equal(expected, actual);
    }

}
