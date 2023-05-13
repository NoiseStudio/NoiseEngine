using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs.Affective;

internal partial class MockAffectiveSystemChildB : EntitySystem {

    public int Value { get; set; }

    private void OnUpdateEntity(ref MockAffectiveComponentA a, ref MockComponentE d) {
        a = a with {
            Precision = (MockAffectivePrecision)(((int)a.Precision + 1) % ((int)MockAffectivePrecision.High + 1))
        };
        d.Value = Value;
    }

}
