using NoiseEngine.Components;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Components;

public class TransformComponentTest {

    [Fact]
    public void EqualsTest() {
        Assert.Equal(new TransformComponent(Float3.Zero), new TransformComponent(Float3.Zero));
        Assert.NotEqual(new TransformComponent(Float3.Zero), new TransformComponent(Float3.One));
    }

}
