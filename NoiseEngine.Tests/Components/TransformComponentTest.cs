using AutoFixture.Xunit2;
using NoiseEngine.Components;
using Xunit;

namespace NoiseEngine.Tests.Components {
    public class TransformComponentTest {

        [Theory, AutoData]
        public void Equals_Self_ReturnsTrue(TransformComponent component) {
            bool result = component.Equals(component);
            Assert.True(result);
        }

        [Theory, AutoData]
        public void Equals_Different_ReturnsFalse(TransformComponent first, TransformComponent second) {
            bool result = first.Equals(second);
            Assert.False(result);
        }

    }
}
