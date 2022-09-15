using NoiseEngine.Interop;

namespace NoiseEngine.Tests.Interop;

public class InteropLibTest {

    [Theory]
    [InlineData(6, 9)]
    public void Add(int left, int right) {
        Assert.Equal(left + right, InteropLib.Add(left, right));
    }

}
