using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

internal class TestSystemA : EntitySystem {

    private int count;

    private void OnUpdateEntity(Entity entity) {
        count++;
    }

}
