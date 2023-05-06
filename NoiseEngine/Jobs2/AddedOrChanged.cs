namespace NoiseEngine.Jobs2;

public readonly record struct AddedOrChanged<T>(T? Old, T Current) where T : IComponent;
