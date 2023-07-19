namespace NoiseEngine.Jobs;

public interface IThreadStorage<TThreadStorage> where TThreadStorage : IThreadStorage<TThreadStorage> {

    /// <summary>
    /// Creates a new instance of <typeparamref name="TThreadStorage"/> for the given <paramref name="entitySystem"/>.
    /// </summary>
    /// <param name="entitySystem">
    /// <see cref="EntitySystem{TThreadStorage}"/> which creates this <typeparamref name="TThreadStorage"/> instance.
    /// </param>
    /// <returns>New not used by other threads <typeparamref name="TThreadStorage"/> instance.</returns>
    public static abstract TThreadStorage Create(EntitySystem<TThreadStorage> entitySystem);

}
