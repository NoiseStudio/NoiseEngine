using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2.Affective;

internal partial class MockAffectiveSystemChildA : EntitySystem {

    public int Value { get; set; }

    private void OnUpdateEntity(MockComponentD d) {
        d.Value = Value;
    }

}
