using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class QuaternionT1Test {

    [Fact]
    public void Dot() {
        Quaternion<float> a = new Quaternion<float>(4, 6, 2, 8);
        Quaternion<float> b = new Quaternion<float>(5, 3, 1, 6);

        Assert.Equal(88, a.Dot(b));
    }

    [Fact]
    public void OperatorMultiplication() {
        Quaternion<float> a = new Quaternion<float>(1, 2, 3, 4);
        Quaternion<float> b = new Quaternion<float>(4, 3, 2, 1);

        Assert.Equal(new Quaternion<float>(12, 24, 6, -12), a * b);
    }

    [Fact]
    public void OperatorMultiplicationByPoint() {
        Quaternion<float> a = new Quaternion<float>(1, 2, 3, 4);
        Vector3<float> b = new Vector3<float>(4, 3, 2);

        Assert.Equal(new Vector3<float>(-116, 63, 2), a * b);
    }

}
