using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

[AppendComponentDefault(typeof(RigidBodyDataComponent))]
public readonly record struct RigidBodyComponent(
    float Mass = 1f,
    Vector3<float> CenterOfMass = default,
    Vector3<float> Velocity = default
) : IComponent;
