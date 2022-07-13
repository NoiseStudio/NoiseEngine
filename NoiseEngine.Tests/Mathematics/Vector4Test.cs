using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class Vector4Test {

    [Fact]
    public void MagnitudeSquared() {
        Vector4<float> a = new Vector4<float>(4, 6, 2, 3);

        Assert.Equal(65, a.MagnitudeSquared());
    }

    [Fact]
    public void Dot() {
        Vector4<float> a = new Vector4<float>(4, 6, 2, 8);
        Vector4<float> b = new Vector4<float>(5, 3, 1, 6);

        Assert.Equal(88, a.Dot(b));
    }

    [Fact]
    public void DistanceSquared() {
        Vector4<float> a = new Vector4<float>(5, 3, 1, 4);
        Vector4<float> b = new Vector4<float>(7, 12, 8, 2);

        Assert.Equal(138, a.DistanceSquared(b));
    }

    [Fact]
    public void Scale() {
        Vector4<float> a = new Vector4<float>(-8, 5, 4, 5);
        Vector4<float> b = new Vector4<float>(3, 2, 6, 3);

        Assert.Equal(new Vector4<float>(-24, 10, 24, 15), a.Scale(b));
    }

    [Fact]
    public void Lerp() {
        Vector4<float> a = new Vector4<float>(1, 2, 3, 8);
        Vector4<float> b = new Vector4<float>(2, 3, 4, 4);

        Assert.Equal(new Vector4<float>(1.5f, 2.5f, 3.5f, 6.0f), a.Lerp(b, 0.5f));
    }

    [Fact]
    public void OperatorAddition() {
        Vector4<float> a = new Vector4<float>(7, 12, 8, 7);
        Vector4<float> b = new Vector4<float>(5, 3, 1, 11);

        Assert.Equal(new Vector4<float>(12, 15, 9, 18), a + b);
    }

    [Fact]
    public void OperatorSubtraction() {
        Vector4<float> a = new Vector4<float>(-8, 5, 4, 3);
        Vector4<float> b = new Vector4<float>(3, 1, 6, 4);

        Assert.Equal(new Vector4<float>(-11, 4, -2, -1), a - b);
    }

    [Fact]
    public void OperatorMultiplication() {
        Vector4<float> a = new Vector4<float>(5, 1, 2, 6);

        Assert.Equal(new Vector4<float>(10, 2, 4, 12), a * 2);
    }

    [Fact]
    public void OperatorDivision() {
        Vector4<float> a = new Vector4<float>(15, 30, 45, 60);

        Assert.Equal(new Vector4<float>(5, 10, 15, 20), a / 3);
    }

    [Fact]
    public void OperatorRemainder() {
        Vector4<float> a = new Vector4<float>(1, 8, 2, 11);

        Assert.Equal(new Vector4<float>(1, 0, 2, 3), a % 4);
    }

}
