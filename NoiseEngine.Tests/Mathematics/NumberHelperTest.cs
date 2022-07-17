using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class NumberHelperTest {

    [Fact]
    public void Half() {
        Assert.Equal(0.5, NumberHelper<double>.Half);
    }

    [Fact]
    public void Two() {
        Assert.Equal(2, NumberHelper<double>.Two);
    }

    [Fact]
    public void Three() {
        Assert.Equal(3, NumberHelper<double>.Three);
    }

    [Fact]
    public void Five() {
        Assert.Equal(5, NumberHelper<double>.Five);
    }

    [Fact]
    public void Value180() {
        Assert.Equal(180, NumberHelper<double>.Value180);
    }

}
