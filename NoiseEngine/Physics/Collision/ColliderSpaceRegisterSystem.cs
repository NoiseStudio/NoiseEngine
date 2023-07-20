using NoiseEngine.Components;
using NoiseEngine.Jobs;
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
        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            transform.Position, transform.Rotation, transform.Scale),
        collider));
    }

}
