using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Rendering;

public class Texture2DTest : GraphicsTestEnvironment {

    private const uint Size = 16;

    private readonly Random random = new Random();

    public Texture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void CreateDestroy() {
        foreach (GraphicsDevice device in Fixture.GraphicsDevices)
            _ = new Texture2D(device, TextureUsage.TransferAll, Size, Size);
    }

    [Fact]
    public void SetGetPixels() {
        SetGetPixelHelper((device, data, result) => {
            Texture2D texture = new Texture2D(device, TextureUsage.TransferAll, Size, Size);
            texture.SetPixels<Color32>(data);
            texture.GetPixels<Color32>(result);
        });
    }

    private void SetGetPixelHelper(Action<GraphicsDevice, Color32[], Color32[]> factory) {
        Color32[] data = CreateRandomPixels();
        Color32[] result = new Color32[data.Length];

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            factory(device, data, result);
            Assert.Equal(result, data);
        }
    }

    private Color32[] CreateRandomPixels(uint width = Size, uint height = Size) {
        Color32[] pixels = new Color32[width * height];
        Span<byte> bytes = stackalloc byte[4];

        for (int i = 0; i < pixels.Length; i++) {
            random.NextBytes(bytes);
            pixels[i] = new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        return pixels;
    }

}
