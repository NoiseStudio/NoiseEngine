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

    [Fact]
    public void DecodePng() {
        byte[] fileData = File.ReadAllBytes("./Resources/Textures/colors.png");
        InteropResult<CpuTextureData> result = CpuTextureInterop.DecodePng(fileData);

        Assert.True(result.IsOk);
        CpuTextureData data = result.Value;

        Assert.Equal<uint>(3, data.ExtentX);
        Assert.Equal<uint>(2, data.ExtentY);
        Assert.Equal<uint>(1, data.ExtentZ);
        Assert.Equal(CpuTextureFormat.R8G8B8, data.Format);

        byte[] expected = GetColorDataRgb(textureColors, new Vector2<uint>(data.ExtentX, data.ExtentY));

        Assert.Equal(expected, data.Data.ToArray());
    }

    private static byte[] GetColorDataRgb(IDictionary<Vector2<uint>, Color32> colors, Vector2<uint> size) {
        byte[] result = new byte[size.X * size.Y * 3];

        int i = 0;

        for (uint y = 0; y < size.Y; y++) {
            for (uint x = 0; x < size.X; x++) {
                result[i + 0] = colors[new Vector2<uint>(x, y)].R;
                result[i + 1] = colors[new Vector2<uint>(x, y)].G;
                result[i + 2] = colors[new Vector2<uint>(x, y)].B;
                i += 3;
            }
        }

        return result;
    }

}
