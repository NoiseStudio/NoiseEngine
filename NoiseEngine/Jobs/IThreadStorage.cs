namespace NoiseEngine.Jobs;

public interface IThreadStorage<TThreadStorage> where TThreadStorage : IThreadStorage<TThreadStorage> {

    public static abstract TThreadStorage Create(EntitySystem<TThreadStorage> entitySystem);

}
