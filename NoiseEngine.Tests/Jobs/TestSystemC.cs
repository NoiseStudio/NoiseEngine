using NoiseEngine.Jobs;
using NoiseEngine.Tests.Jobs.Affective;

namespace NoiseEngine.Tests.Jobs;

internal partial class TestSystemC : EntitySystem {

    private void OnUpdateEntity(ref MockComponentE e, ref MockAffectiveComponentA a) {
        e.Value *= 2;
        a = MockAffectiveComponentA.High;
    }

}
