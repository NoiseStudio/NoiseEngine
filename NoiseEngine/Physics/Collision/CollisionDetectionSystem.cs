using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Physics.Collision.Sphere;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionDetectionSystem : EntitySystem<CollisionDetectionThreadStorage> {

    private readonly CollisionSpace space;

    public CollisionDetectionSystem(CollisionSpace space) {
        this.space = space;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void FromSphere(
        Entity entity, CollisionDetectionThreadStorage storage, SphereCollider current,
        ColliderTransform currentTransform
    ) {
        foreach (ConcurrentBag<ColliderData> bag in storage.ColliderDataBuffer) {
            foreach (ColliderData other in bag) {
                if (entity == other.Entity)
                    continue;

                switch (other.Collider.Type) {
                    case ColliderType.Sphere:
                        SphereToSphere.Collide(
                            current, currentTransform, other.Collider.UnsafeCastToSphereCollider(), other.Transform
                        );
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    protected override void OnLateUpdate() {
        space.ClearColliders();
    }

    private void OnUpdateEntity(
        Entity entity, CollisionDetectionThreadStorage storage, TransformComponent transform,
        RigidBodyDataComponent data, ColliderComponent collider
    ) {
        ColliderTransform currentTransform = new ColliderTransform(
            data.TargetPosition, transform.Rotation, transform.Scale
        );
        space.GetNearColliders(storage.ColliderDataBuffer);

        switch (collider.Type) {
            case ColliderType.Sphere:
                FromSphere(entity, storage, collider.UnsafeCastToSphereCollider(), currentTransform);
                break;
            default:
                throw new NotImplementedException();
        }

        storage.ColliderDataBuffer.Clear();
    }

}
