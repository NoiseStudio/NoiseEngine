using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Tests.Mathematics;

public class QuaternionExtensionsTest {

    [Fact]
    public void ToEulerRadians() {
        Quaternion<float> a = new Quaternion<float>(0.3122061f, 0.4274071f, 0.3122061f, 0.7889094f);

        Assert.Equal(new Vector3<float>(1.046f, 0.50000006f, 1.046f), a.ToEulerRadians());
    }

    [Fact]
    public void ToEulerDegrees() {
        Quaternion<float> a = new Quaternion<float>(0.3122061f, 0.4274071f, 0.3122061f, 0.7889094f);

        Assert.Equal(new Vector3<float>(59.9313854f, 28.647892f, 59.9313854f), a.ToEulerDegress());
    }

    [Fact]
    public void AngleRadians() {
        Quaternion<float> a = new Quaternion<float>(0.3f, 1, 0.3f, 0);
        Quaternion<float> b = new Quaternion<float>(0, 0, 0, 0.5f);

        Assert.Equal(MathF.PI, a.AngleRadians(b));
    }

    [Fact]
    public void AngleDegrees() {
        Quaternion<float> a = new Quaternion<float>(0.3f, 1, 0.3f, 0);
        Quaternion<float> b = new Quaternion<float>(0, 0, 0, 0.5f);

        Assert.Equal(90, a.AngleDegrees(b));
    }

    [Fact]
    public void Normalize() {
        Quaternion<float> a = new Quaternion<float>(3, 1, 2, 6);

        Assert.Equal(new Quaternion<float>(0.42426407f, 0.14142136f, 0.28284273f, 0.84852815f), a.Normalize());
    }

}
