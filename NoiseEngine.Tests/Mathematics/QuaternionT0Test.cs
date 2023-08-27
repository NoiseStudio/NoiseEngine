using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class QuaternionT0Test {

    [Fact]
    public void EulerRadians() {
        Quaternion<float> a = Quaternion.EulerRadians(14.0f, -42.6f, 256.0f);
        Assert.Equal(new Quaternion<float>(
            0.69725597f, -0.032025933f, -0.7090168f, 0.10054487f
        ), a);
    }

}
