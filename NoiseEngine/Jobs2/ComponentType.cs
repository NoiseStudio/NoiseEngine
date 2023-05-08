using System;

namespace NoiseEngine.Jobs2;

public readonly record struct ComponentType(Type Type, int AffectiveHashCode);
