using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal partial class TestSystemB : EntitySystem {

    private void OnUpdateEntity(ref MockComponentE e) {
        e.Value *= 2;
    }

}
