using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class QuaternionT0Test {

    [Theory]
    [InlineData(
        new float[] { 14f, -42.6f, 256f },
        new float[] { 0.69725597f, -0.032025933f, -0.7090168f, 0.10054487f }
    )]
    public void EulerRadians(float[] init, float[] expected) {
        Quaternion<float> a = Quaternion.EulerRadians(init[0], init[1], init[2]);
        Assert.Equal(new Quaternion<float>(
            expected[0], expected[1], expected[2], expected[3]
        ), a);
    }

    [Theory]
    [InlineData(
        new float[] { 0.81f, 0.59f, -0.06f, 0.34f, 0.0f, 0.94f },
        new float[] { 0.39524522f, 0.61128134f, 0.6399186f, 0.24621218f }
    )]
    [InlineData(
        new float[] { 0.13f, -0.04f, -0.99f },
        new float[] { 0.0013059094f, 0.9976699f, -0.019975385f, 0.065223604f }
    )]
    public void LookRotation(float[] init, float[] expected) {
        Quaternion<float> a = Quaternion.LookRotation(
            new float3(init[0], init[1], init[2]),
            init.Length > 3 ? new float3(init[3], init[4], init[5]) : float3.Up
        );
        Assert.Equal(new Quaternion<float>(
            expected[0], expected[1], expected[2], expected[3]
        ), a);
    }

}
