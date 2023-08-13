using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Physics.Collision;

internal partial class RigidBodySleepingColliderRegisterSystem : EntitySystem {

    private readonly CollisionSpace space;

    public RigidBodySleepingColliderRegisterSystem(CollisionSpace space) {
        this.space = space;
        Filter = new EntityFilter(new Type[] { typeof(RigidBodyComponent), typeof(RigidBodySleepComponent) });
    }

    private void OnUpdateEntity(Entity entity, TransformComponent transform, ColliderComponent collider) {
        // Immovable objects have infinite mass.
        // 1f / inf == 0f
        const float InverseMass = 0f;

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            transform.Position,
            default, // World center of mass is not needed for immovable objects.
            transform.Scale,
            Vector3<float>.Zero,
            default, // Inverse inertia tensor matrix is not needed for immovable objects.
            InverseMass,
            true
        ), collider));
    }

}
