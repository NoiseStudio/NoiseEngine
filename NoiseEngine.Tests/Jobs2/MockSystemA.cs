using NoiseEngine.Jobs2;
using System;

namespace NoiseEngine.Tests.Jobs2;

internal partial class MockSystemA : EntitySystem, IAffectiveSystem<MockSystemA, MockComponentD> {

    static (MockSystemA instance, double cycleTime) IAffectiveSystem<MockSystemA, MockComponentD>.Construct(
        MockComponentD componentA
    ) {
        return (new MockSystemA(), componentA.Precision switch {
            MockAffectivePrecision.Low => 300,
            MockAffectivePrecision.Medium => 150,
            MockAffectivePrecision.High => 50,
            _ => throw new ArgumentOutOfRangeException(nameof(componentA), componentA, null)
        });
    }

    private void OnUpdateEntity(ref MockComponentA componentA, MockComponentB componentB) {
        componentA = componentA with { Text = componentB.Text };
    }

}
