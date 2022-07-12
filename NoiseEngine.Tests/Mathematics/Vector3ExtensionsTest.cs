using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Tests.Mathematics;

public class Vector3ExtensionsTest {

    [Fact]
    public void Magnitude() {
        Vector3<float> a = new Vector3<float>(4, 6, 2);

        Assert.Equal(MathF.Sqrt(56), a.Magnitude());
    }

    [Fact]
    public void Normalize() {
        Vector3<float> a = new Vector3<float>(3, 1, 2);

        Assert.Equal(new Vector3<float>(0.8017837f, 0.26726124f, 0.5345225f), a.Normalize());
    }

    [Fact]
    public void Distance() {
        Vector3<float> a = new Vector3<float>(5, 3, 1);
        Vector3<float> b = new Vector3<float>(7, 12, 8);

        Assert.Equal(MathF.Sqrt(134), a.Distance(b));
    }

}
