using NoiseEngine.Mathematics.Helpers;

namespace NoiseEngine.Tests.Mathematics;

public class NumberHelperTest {

    [Fact]
    public void Half() {
        Assert.Equal(0.5, NumberHelper<double>.Half);
    }

    [Fact]
    public void Two() {
        Assert.Equal(2, NumberHelper<double>.Value2);
    }

    [Fact]
    public void Three() {
        Assert.Equal(3, NumberHelper<double>.Value3);
    }

    [Fact]
    public void Five() {
        Assert.Equal(5, NumberHelper<double>.Value5);
    }

    [Fact]
    public void Value180() {
        Assert.Equal(180, NumberHelper<double>.Value180);
    }

}
