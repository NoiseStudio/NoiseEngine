using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Tests.Mathematics;

public class Vector4ExtensionsTest {

    [Fact]
    public void Magnitude() {
        Vector4<float> a = new Vector4<float>(4, 6, 2, 3);

        Assert.Equal(MathF.Sqrt(65), a.Magnitude());
    }

    [Fact]
    public void Normalize() {
        Vector4<float> a = new Vector4<float>(3, 1, 2, 6);

        Assert.Equal(new Vector4<float>(0.42426407f, 0.14142136f, 0.28284273f, 0.84852815f), a.Normalize());
    }

    [Fact]
    public void Distance() {
        Vector4<float> a = new Vector4<float>(5, 3, 1, 4);
        Vector4<float> b = new Vector4<float>(7, 12, 8, 2);

        Assert.Equal(MathF.Sqrt(138), a.Distance(b));
    }

}
