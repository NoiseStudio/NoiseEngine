using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs.Affective;

internal partial class MockAffectiveSystemChildA : EntitySystem {

    public int Value { get; set; }

    private void OnUpdateEntity(MockComponentD d) {
        d.Value = Value;
    }

}
