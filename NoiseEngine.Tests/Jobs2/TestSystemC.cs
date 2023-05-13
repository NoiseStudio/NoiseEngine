using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Jobs2.Affective;

namespace NoiseEngine.Tests.Jobs2;

internal partial class TestSystemC : EntitySystem {

    private void OnUpdateEntity(ref MockComponentE e, ref MockAffectiveComponentA a) {
        e.Value *= 2;
        a = MockAffectiveComponentA.High;
    }

}
