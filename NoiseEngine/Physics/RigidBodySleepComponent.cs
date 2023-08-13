using NoiseEngine.Jobs;

namespace NoiseEngine.Physics;

internal readonly record struct RigidBodySleepComponent(bool WakeUp) : IComponent;
