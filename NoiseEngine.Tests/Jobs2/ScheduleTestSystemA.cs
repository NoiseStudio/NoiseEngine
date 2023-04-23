using NoiseEngine.Jobs2;
using System;
using System.Diagnostics;
using System.Threading;

namespace NoiseEngine.Tests.Jobs2;

internal partial class ScheduleTestSystemA : EntitySystem {

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

    protected override void OnLateUpdate() {
        Assert.True(IsWorking);

        UsedLateUpdate = true;
        LateUpdateCount++;
        LateUpdateResetEvent.Set();
    }

    protected override void OnTerminate() {
        LateUpdateResetEvent.Dispose();
    }

    private void OnUpdateEntity(Entity entity) {
        Interlocked.Increment(ref updateEntityCount);
        entity.Despawn();
    }

}
