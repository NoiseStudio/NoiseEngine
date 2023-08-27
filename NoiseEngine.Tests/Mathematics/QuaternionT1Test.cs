using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class QuaternionT1Test {

    [Fact]
    public void Conjugate() {
        Quaternion<float> a = Quaternion.EulerRadians(14.0f, -42.6f, 256.0f);
        Assert.Equal(new Quaternion<float>(-0.69725597f, 0.032025933f, 0.7090168f, 0.10054487f), a.Conjugate());
    }

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

    [Theory]
    [InlineData(new float[] { 1, 2, 3, 4, 4, 3, 2, -116, 63, 2 })]
    [InlineData(new float[] {
        0.69725597f, -0.032025933f, -0.7090168f, 0.10054487f, 2f, 2f, 2f, -1.809436f, -2.51955f, -1.542101f
    })]
    public void OperatorMultiplicationByPoint(float[] data) {
        Quaternion<float> a = new Quaternion<float>(data[0], data[1], data[2], data[3]);
        float3 b = new float3(data[4], data[5], data[6]);

        Assert.Equal(new float3(data[7], data[8], data[9]), a * b);
    }

}
