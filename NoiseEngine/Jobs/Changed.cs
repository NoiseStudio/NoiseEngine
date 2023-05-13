namespace NoiseEngine.Jobs;

public readonly record struct Changed<T>(T Old, T Current) where T : IComponent;
