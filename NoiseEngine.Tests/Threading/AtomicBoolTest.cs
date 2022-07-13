using NoiseEngine.Threading;

namespace NoiseEngine.Tests.Threading;

public class AtomicBoolTest {

    [Fact]
    public void Exchange() {
        AtomicBool b = new AtomicBool();
        Assert.False(b.Exchange(true));
        Assert.True(b.Exchange(true));
        Assert.True(b.Exchange(false));
    }

}
