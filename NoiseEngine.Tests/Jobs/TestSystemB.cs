using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal partial class TestSystemB : EntitySystem {

    private void OnUpdateEntity(ref MockComponentE e) {
        e.Value *= 2;
    }

}
