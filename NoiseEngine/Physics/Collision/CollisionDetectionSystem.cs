using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Physics.Collision.Sphere;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionDetectionSystem : EntitySystem<CollisionDetectionThreadStorage> {

    private readonly CollisionSpace space;
    private readonly ContactPointsBuffer buffer;

    public CollisionDetectionSystem(CollisionSpace space, ContactPointsBuffer buffer) {
        this.space = space;
        this.buffer = buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void FromSphere(
        Entity entity, CollisionDetectionThreadStorage storage, SphereCollider current,
        float currentRestitutionPlusOneNegative, ColliderTransform currentTransform
    ) {
        foreach (ConcurrentBag<ColliderData> bag in storage.ColliderDataBuffer) {
            foreach (ColliderData other in bag) {
                if (entity == other.Entity)
                    continue;

                switch (other.Collider.Type) {
                    case ColliderType.Sphere:
                        SphereToSphere.Collide(
                            buffer, current, currentRestitutionPlusOneNegative, currentTransform, entity,
                            other.Collider.UnsafeCastToSphereCollider(), other.Collider.RestitutionPlusOneNegative,
                            other.Transform
                        );
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    protected override void OnUpdate() {
        buffer.Clear();
    }

    protected override void OnLateUpdate() {
        space.ClearColliders();
    }

    private void OnUpdateEntity(
        Entity entity, CollisionDetectionThreadStorage storage, TransformComponent transform,
        RigidBodyComponent rigidBody, RigidBodyMiddleDataComponent middle, ColliderComponent collider
    ) {
        ColliderTransform currentTransform = new ColliderTransform(
            middle.Position, middle.Position + rigidBody.CenterOfMass, transform.Scale, rigidBody.LinearVelocity,
            rigidBody.InverseInertiaTensorMatrix, rigidBody.InverseMass
        );
        space.GetNearColliders(storage.ColliderDataBuffer);

        switch (collider.Type) {
            case ColliderType.Sphere:
                FromSphere(
                    entity, storage, collider.UnsafeCastToSphereCollider(), collider.RestitutionPlusOneNegative,
                    currentTransform
                );
                break;
            default:
                throw new NotImplementedException();
        }

        storage.ColliderDataBuffer.Clear();
    }

}
