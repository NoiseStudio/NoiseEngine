using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

internal class TestSystemA : EntitySystem {

    private int count;

    protected override void OnUpdateEntity(Entity entity) {
        count++;
    }

}
