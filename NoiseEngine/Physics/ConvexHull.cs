using System;

namespace NoiseEngine.Physics;

public readonly record struct ConvexHull(ReadOnlyMemory<float3> Vertices, float3 SphereCenter, float SphereRadius);
