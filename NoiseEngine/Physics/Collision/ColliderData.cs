using NoiseEngine.Jobs;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct ColliderData(
    Entity Entity,
    ColliderTransform Transform,
    ColliderComponent Collider
);
