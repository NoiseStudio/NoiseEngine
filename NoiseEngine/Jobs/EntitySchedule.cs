using System;

namespace NoiseEngine.Jobs;

public class EntitySchedule : IDisposable {

    [ThreadStatic]
    internal static bool isScheduleLockThread;

    internal EntityScheduleWorker Worker { get; }

    public EntitySchedule(int? threadCount = null) {
        Worker = new EntityScheduleWorker(threadCount);
    }

    ~EntitySchedule() {
        Worker.Dispose();
    }

    internal static void AssertNotScheduleLockThread(string helpMessage) {
        if (isScheduleLockThread) {
            throw new InvalidOperationException(
                "Calling this method from the schedule thread is not allowed because this can cause a deadlocks. " +
                helpMessage
            );
        }
    }

    /// <summary>
    /// Disposes this <see cref="EntitySchedule"/>.
    /// </summary>
    public void Dispose() {
        Worker.Dispose();
        GC.SuppressFinalize(this);
    }

}
