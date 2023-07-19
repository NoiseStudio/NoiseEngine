using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal class MockThreadStorageA : IThreadStorage<MockThreadStorageA> {

    public int Count { get; set; }

    public static MockThreadStorageA Create(EntitySystem<MockThreadStorageA> entitySystem) {
        return new MockThreadStorageA();
    }

}
