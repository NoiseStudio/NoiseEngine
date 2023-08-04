using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Linq;

namespace NoiseEngine.Physics.Collision;

internal partial class ColliderSpaceRegisterSystem : EntitySystem {

    private readonly CollisionSpace space;

    public ColliderSpaceRegisterSystem(CollisionSpace space) {
        this.space = space;
        Filter = new EntityFilter(Enumerable.Empty<Type>(), new Type[] { typeof(RigidBodyComponent) });
    }

    private void OnUpdateEntity(Entity entity, TransformComponent transform, ColliderComponent collider) {
        // Immovable objects have infinite mass.
        // 1f / inf == 0f
        const float InverseMass = 0f;

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            transform.Position, transform.Rotation, transform.Scale, Vector3<float>.Zero, InverseMass, -1.5f
        ), collider));
    }

}
