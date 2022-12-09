using NoiseEngine.Jobs;
using System.Threading;

namespace NoiseEngine.Tests.Jobs;

internal class TestSystemScheduleA : EntitySystem<TestComponentA, TestComponentB> {

    private int updateEntityCount;

    public int UpdateEntityCount => updateEntityCount;

    public bool UsedUpdate { get; private set; }
    public bool UsedLateUpdate { get; private set; }
    public int UpdateCount { get; private set; }
    public int LateUpdateCount { get; private set; }

    public AutoResetEvent LateUpdateResetEvent { get; } = new AutoResetEvent(false);

    protected override void OnUpdate() {
        UsedUpdate = true;
        UpdateCount++;

        Assert.True(IsWorking);
    }

    protected override void OnUpdateEntity(Entity entity, TestComponentA component1, TestComponentB component2) {
        Interlocked.Increment(ref updateEntityCount);
        entity.Destroy(World);
    }

    protected override void OnLateUpdate() {
        Assert.True(IsWorking);

        UsedLateUpdate = true;
        LateUpdateCount++;
        LateUpdateResetEvent.Set();
    }

    protected override void OnTerminate() {
        LateUpdateResetEvent.Dispose();
    }

}
