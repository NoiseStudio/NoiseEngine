using NoiseEngine.Jobs;
using System;
using System.Collections.Concurrent;

namespace NoiseEngine.Tests.Jobs;

public class JobsFixture : IDisposable {

    public EntitySchedule EntitySchedule { get; }
    public EntityWorld EntityWorld { get; }

    public JobsInvoker JobsInvoker { get; }
    public JobsWorld JobsWorld { get; }
    public JobsWorld JobsWorldFast { get; }

    public Entity NextEmptyEntity => EntityWorld.NewEntity();
    public EntityWorld EmptyEntityWorld => new EntityWorld();

    public JobsFixture() {
        EntitySchedule = new EntitySchedule();
        EntityWorld = new EntityWorld();

        JobsInvoker = new JobsInvoker();
        JobsWorld = new JobsWorld(JobsInvoker);
        JobsWorldFast = new JobsWorld(JobsInvoker, new uint[] {
            2, 3, 5, 10
        });
    }

    public void Dispose() {
        EntityWorld.Dispose();
        EntitySchedule.Dispose();

        JobsInvoker.Dispose();
        JobsWorld.Dispose();
        JobsWorldFast.Dispose();
    }

}
