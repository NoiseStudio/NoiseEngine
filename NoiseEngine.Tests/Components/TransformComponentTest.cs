using NoiseEngine.Components;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Components;

public class TransformComponentTest {

    [Fact]
    public void EqualsTest() {
        Assert.Equal(new TransformComponent(Vector3<float>.Zero), new TransformComponent(Vector3<float>.Zero));
        Assert.NotEqual(new TransformComponent(Vector3<float>.Zero), new TransformComponent(Vector3<float>.One));
    }

}
