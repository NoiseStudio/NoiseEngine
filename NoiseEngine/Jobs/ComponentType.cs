using System;

namespace NoiseEngine.Jobs;

public readonly record struct ComponentType(Type Type, int AffectiveHashCode);
