using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Tests.Mathematics;

public class Vector2ExtensionsTest {

    [Fact]
    public void Magnitude() {
        Vector2<float> a = new Vector2<float>(4, 6);

        Assert.Equal(MathF.Sqrt(52), a.Magnitude());
    }

    [Fact]
    public void Normalize() {
        Vector2<float> a = new Vector2<float>(3, 1);

        Assert.Equal(new Vector2<float>(0.94868326f, 0.31622776f), a.Normalize());
    }

    [Fact]
    public void Distance() {
        Vector2<float> a = new Vector2<float>(5, 3);
        Vector2<float> b = new Vector2<float>(7, 12);

        Assert.Equal(MathF.Sqrt(85), a.Distance(b));
    }

}
