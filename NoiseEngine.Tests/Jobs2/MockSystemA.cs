using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal partial class MockSystemA : EntitySystem {

    private void OnUpdateEntity(ref MockComponentA componentA, MockComponentB componentB) {
        componentA = componentA with { Text = componentB.Text };
    }

}
