namespace NoiseEngine.Physics.Collision.Mesh;

internal readonly record struct ConvexHullId(int StartIndex, int EndIndex, float3 SphereCenter, float SphereRadius);
