using NoiseEngine.Jobs;
using System.Threading;

namespace NoiseEngine.Tests.Jobs;

internal partial class ScheduleTestSystemA : EntitySystem {

    private int updateEntityCount;

    public int UpdateEntityCount => updateEntityCount;

    public bool UsedUpdate { get; private set; }
    public bool UsedLateUpdate { get; private set; }
    public int LateUpdateCount { get; private set; }

    public int UpdateCount;
    public int DisposeCount;

    public AutoResetEvent LateUpdateResetEvent { get; } = new AutoResetEvent(false);

    protected override void OnUpdate() {
        UsedUpdate = true;
        Interlocked.Increment(ref UpdateCount);

        Assert.True(IsWorking);
    }

    protected override void OnLateUpdate() {
        Assert.True(IsWorking);

        UsedLateUpdate = true;
        LateUpdateCount++;
        LateUpdateResetEvent.Set();
    }

    protected override void OnDispose() {
        LateUpdateResetEvent.Dispose();
        Interlocked.Increment(ref DisposeCount);
    }

    private void OnUpdateEntity(Entity entity) {
        Interlocked.Increment(ref updateEntityCount);
        entity.Despawn();
    }

}
