using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class Vector2Test {

    [Fact]
    public void MagnitudeSquared() {
        Vector2<float> a = new Vector2<float>(4, 6);

        Assert.Equal(52, a.MagnitudeSquared());
    }

    [Fact]
    public void Dot() {
        Vector2<float> a = new Vector2<float>(4, 6);
        Vector2<float> b = new Vector2<float>(5, 3);

        Assert.Equal(38, a.Dot(b));
    }

    [Fact]
    public void DistanceSquared() {
        Vector2<float> a = new Vector2<float>(5, 3);
        Vector2<float> b = new Vector2<float>(7, 12);

        Assert.Equal(85, a.DistanceSquared(b));
    }

    [Fact]
    public void Scale() {
        Vector2<float> a = new Vector2<float>(-8, 5);
        Vector2<float> b = new Vector2<float>(3, 2);

        Assert.Equal(new Vector2<float>(-24, 10), a.Scale(b));
    }

    [Fact]
    public void Lerp() {
        Vector2<float> a = new Vector2<float>(1, 2);
        Vector2<float> b = new Vector2<float>(2, 3);

        Assert.Equal(new Vector2<float>(1.5f, 2.5f), a.Lerp(b, 0.5f));
    }

    [Fact]
    public void OperatorAddition() {
        Vector2<float> a = new Vector2<float>(7, 12);
        Vector2<float> b = new Vector2<float>(5, 3);

        Assert.Equal(new Vector2<float>(12, 15), a + b);
    }

    [Fact]
    public void OperatorSubtraction() {
        Vector2<float> a = new Vector2<float>(-8, 5);
        Vector2<float> b = new Vector2<float>(3, 1);

        Assert.Equal(new Vector2<float>(-11, 4), a - b);
    }

    [Fact]
    public void OperatorMultiplication() {
        Vector2<float> a = new Vector2<float>(5, 1);

        Assert.Equal(new Vector2<float>(10, 2), a * 2);
    }

    [Fact]
    public void OperatorDivision() {
        Vector2<float> a = new Vector2<float>(15, 30);

        Assert.Equal(new Vector2<float>(5, 10), a / 3);
    }

    [Fact]
    public void OperatorRemainder() {
        Vector2<float> a = new Vector2<float>(1, 8);

        Assert.Equal(new Vector2<float>(1, 2), a % 3);
    }

}
