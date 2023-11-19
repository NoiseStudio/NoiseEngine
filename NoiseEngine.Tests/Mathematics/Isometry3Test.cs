using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class Isometry3Test {

    [Fact]
    public void InverseMultiplication() {
        Isometry3<float> a = new Isometry3<float>(
            new float3(53.1f, 14.6f, -74.1f),
            Quaternion.EulerRadians(14.0f, -42.6f, 256.0f)
        );
        Isometry3<float> b = new Isometry3<float>(
            new float3(13.46f, 334.23f, -256.33f),
            Quaternion.EulerRadians(-67.0f, 684.23f, 83.23f)
        );

        Isometry3<float> result = a.InverseMultiplication(b);
        Assert.Equal(new Isometry3<float>(
            new float3(119.452446f, -350.22327f, 4.4804454f),
            new Quaternion<float>(0.34461325f, -0.91064215f, 0.19555788f, -0.11720917f)
        ), result);
    }

    [Fact]
    public void OperatorMultiplicationByPoint() {
        Isometry3<float> a = new Isometry3<float>(
            new float3(53.1f, 14.6f, -74.1f),
            Quaternion.EulerRadians(14.0f, -42.6f, 256.0f)
        );

        float3 result = a * new float3(5.36f, -156.4f, 456.3f);
        Assert.Equal(new float3(-416.35117f, 123.25922f, -96.7078f), result);
    }

}
