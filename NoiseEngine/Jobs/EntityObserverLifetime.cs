using System;

namespace NoiseEngine.Jobs;

public sealed class EntityObserverLifetime : IDisposable {

    private readonly EntityObserverType type;
    private Delegate? observer;

    public EntityWorld World { get; }

    internal EntityObserverLifetime(EntityWorld world, EntityObserverType type, Delegate observer) {
        World = world;
        this.type = type;
        this.observer = observer;
    }

    ~EntityObserverLifetime() {
        ReleaseResouces();
    }

    /// <summary>
    /// Disposes this Entity Observer.
    /// </summary>
    public void Dispose() {
        ReleaseResouces();
        GC.SuppressFinalize(this);
    }

    private void ReleaseResouces() {
        Delegate? observer = this.observer;
        if (observer is null)
            return;
        this.observer = null;

        switch (type) {
            case EntityObserverType.Changed:
                World.RemoveChangedObserver(observer);
                break;
            default:
                throw new NotImplementedException();
        }
    }

}
