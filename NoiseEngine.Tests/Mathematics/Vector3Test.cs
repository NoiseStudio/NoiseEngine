using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class Vector3Test {

    [Fact]
    public void MagnitudeSquared() {
        Vector3<float> a = new Vector3<float>(4, 6, 2);

        Assert.Equal(56, a.MagnitudeSquared());
    }

    [Fact]
    public void Dot() {
        Vector3<float> a = new Vector3<float>(4, 6, 2);
        Vector3<float> b = new Vector3<float>(5, 3, 1);

        Assert.Equal(40, a.Dot(b));
    }

    [Fact]
    public void DistanceSquared() {
        Vector3<float> a = new Vector3<float>(5, 3, 1);
        Vector3<float> b = new Vector3<float>(7, 12, 8);

        Assert.Equal(134, a.DistanceSquared(b));
    }

    [Fact]
    public void Scale() {
        Vector3<float> a = new Vector3<float>(-8, 5, 4);
        Vector3<float> b = new Vector3<float>(3, 2, 6);

        Assert.Equal(new Vector3<float>(-24, 10, 24), a.Scale(b));
    }

    [Fact]
    public void Lerp() {
        Vector3<float> a = new Vector3<float>(1, 2, 3);
        Vector3<float> b = new Vector3<float>(2, 3, 4);

        Assert.Equal(new Vector3<float>(1.5f, 2.5f, 3.5f), a.Lerp(b, 0.5f));
    }

    [Theory]
    [InlineData(new float[] { 7, -3, 5, 4, 3, 2 }, new float[] { -21, 6, 33 })]
    [InlineData(new float[] { 1, 2, 3, 1, 5, 7 }, new float[] { -1, -4, 3 })]
    [InlineData(new float[] { -1, -2, 3, 4, 0, -8 }, new float[] { 16, 4, 8 })]
    public void Cross(float[] init, float[] expected) {
        float3 a = new float3(init[0], init[1], init[2]);
        float3 b = new float3(init[3], init[4], init[5]);

        Assert.Equal(new float3(expected[0], expected[1], expected[2]), a.Cross(b));
    }

    [Fact]
    public void OperatorAddition() {
        Vector3<float> a = new Vector3<float>(7, 12, 8);
        Vector3<float> b = new Vector3<float>(5, 3, 1);

        Assert.Equal(new Vector3<float>(12, 15, 9), a + b);
    }

    [Fact]
    public void OperatorSubtraction() {
        Vector3<float> a = new Vector3<float>(-8, 5, 4);
        Vector3<float> b = new Vector3<float>(3, 1, 6);

        Assert.Equal(new Vector3<float>(-11, 4, -2), a - b);
    }

    [Fact]
    public void OperatorMultiplication() {
        Vector3<float> a = new Vector3<float>(5, 1, 2);

        Assert.Equal(new Vector3<float>(10, 2, 4), a * 2);
    }

    [Fact]
    public void OperatorDivision() {
        Vector3<float> a = new Vector3<float>(15, 30, 45);

        Assert.Equal(new Vector3<float>(5, 10, 15), a / 3);
    }

    [Fact]
    public void OperatorRemainder() {
        Vector3<float> a = new Vector3<float>(1, 8, 3);

        Assert.Equal(new Vector3<float>(1, 2, 0), a % 3);
    }

}
