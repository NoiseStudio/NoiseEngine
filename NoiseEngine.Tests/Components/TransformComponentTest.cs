using NoiseEngine.Components;

namespace NoiseEngine.Tests.Components;

public class TransformComponentTest {

    [Fact]
    public void EqualsTest() {
        Assert.Equal(new TransformComponent(pos3.Zero), new TransformComponent(pos3.Zero));
        Assert.NotEqual(new TransformComponent(pos3.Zero), new TransformComponent(pos3.One));
    }

}
