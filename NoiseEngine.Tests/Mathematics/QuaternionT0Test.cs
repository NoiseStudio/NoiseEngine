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

}
