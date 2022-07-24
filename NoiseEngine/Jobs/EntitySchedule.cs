using System;
using System.Collections.Concurrent;

namespace NoiseEngine.Jobs;

public class EntitySchedule : IDisposable {

    private readonly EntityScheduleWorker worker;

    internal ConcurrentDictionary<int, int> ThreadIds => worker.threadIds;
    internal int ThreadIdCount => worker.threadIdCount;

    /// <summary>
    /// Creates new Entity Schedule
    /// </summary>
    /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
    /// <param name="maxPackageSize">The maximum size of an UpdateEntity package shared between threads</param>
    /// <param name="minPackageSize">The minimum size of an UpdateEntity package shared between threads</param>
    /// <exception cref="InvalidOperationException">Error when using zero or negative threads and when the minimum package size is greater than the maximum package size</exception>
    public EntitySchedule(int? threadCount = null, int? maxPackageSize = null, int? minPackageSize = null) {
        worker = new EntityScheduleWorker(threadCount, maxPackageSize, minPackageSize);
    }

    ~EntitySchedule() {
        ReleaseResources();
    }

    /// <summary>
    /// Disposes this <see cref="EntitySchedule"/>.
    /// </summary>
    public void Dispose() {
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    internal void AddSystem(EntitySystemBase system) {
        worker.AddSystem(system);
    }

    internal void RemoveSystem(EntitySystemBase system) {
        worker.RemoveSystem(system);
    }

    internal bool HasSystem(EntitySystemBase system) {
        return worker.HasSystem(system);
    }

    internal void EnqueuePackages(EntitySystemBase system) {
        worker.EnqueuePackages(system);
    }

    private void ReleaseResources() {
        worker.Dispose();
    }

}
