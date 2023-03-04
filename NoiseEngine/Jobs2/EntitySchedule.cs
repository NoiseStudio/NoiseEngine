using System;

namespace NoiseEngine.Jobs2;

public class EntitySchedule : IDisposable {

    internal EntityScheduleWorker Worker { get; }

    public EntitySchedule(int? threadCount = null) {
        Worker = new EntityScheduleWorker(threadCount);
    }

    ~EntitySchedule() {
        Worker.Dispose();
    }

    /// <summary>
    /// Disposes this <see cref="EntitySchedule"/>.
    /// </summary>
    public void Dispose() {
        Worker.Dispose();
        GC.SuppressFinalize(this);
    }

}
