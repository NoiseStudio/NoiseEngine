using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal partial class TestSystemD : EntitySystem<MockThreadStorageA> {

    public int Count { get; private set; }

    protected override void OnLateUpdate() {
        Count = 0;
        foreach (MockThreadStorageA storage in ThreadStorages) {
            Count += storage.Count;
            storage.Count = 0;
        }
    }

    private void OnUpdateEntity(Entity entity, MockThreadStorageA storage) {
        storage.Count++;
        entity.Despawn();
    }

}
