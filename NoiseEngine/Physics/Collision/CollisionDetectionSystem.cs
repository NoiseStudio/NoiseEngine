using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision.Mesh;
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
        Entity entity, SystemCommands commands, CollisionDetectionThreadStorage storage, in SphereCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform
    ) {
        foreach (ConcurrentBag<ColliderData> bag in storage.ColliderDataBuffer) {
            foreach (ColliderData other in bag) {
                if (entity == other.Entity)
                    continue;

                switch (other.Collider.Type) {
                    case ColliderType.Sphere:
                        SphereToSphere.Collide(
                            commands, buffer, current, currentRestitutionPlusOneNegative, currentTransform, entity,
                            other.Collider.UnsafeCastToSphereCollider(), other.Collider.RestitutionPlusOneNegative,
                            other.Transform, other.Entity
                        );
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void FromMesh(
        Entity entity, SystemCommands commands, CollisionDetectionThreadStorage storage, in MeshCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform
    ) {
        foreach (ConcurrentBag<ColliderData> bag in storage.ColliderDataBuffer) {
            foreach (ColliderData other in bag) {
                if (entity == other.Entity)
                    continue;

                switch (other.Collider.Type) {
                    case ColliderType.Mesh:
                        MeshToMesh.Collide(
                            World, commands, buffer, current, currentRestitutionPlusOneNegative, currentTransform, entity,
                            other.Collider.UnsafeCastToMeshCollider(), other.Collider.RestitutionPlusOneNegative,
                            other.Transform, other.Entity
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
        Entity entity, SystemCommands commands, CollisionDetectionThreadStorage storage, TransformComponent transform,
        RigidBodyComponent rigidBody, RigidBodyMiddleDataComponent middle, RigidBodyFinalDataComponent final,
        ColliderComponent collider
    ) {
        if (rigidBody.IsSleeping)
            return;

        ColliderTransform currentTransform = new ColliderTransform(
            middle.Position, final.TargetRotation, middle.Position + rigidBody.CenterOfMass.ToPos(), transform.Scale,
            rigidBody.LinearVelocity, rigidBody.AngularVelocity, rigidBody.InverseInertiaTensorMatrix, rigidBody.InverseMass, true
        );
        space.GetNearColliders(storage.ColliderDataBuffer);

        switch (collider.Type) {
            case ColliderType.Sphere:
                FromSphere(
                    entity, commands, storage, collider.UnsafeCastToSphereCollider(),
                    collider.RestitutionPlusOneNegative, currentTransform
                );
                break;
            case ColliderType.Mesh:
                FromMesh(
                    entity, commands, storage, collider.UnsafeCastToMeshCollider(),
                    collider.RestitutionPlusOneNegative, currentTransform
                );
                break;
            default:
                throw new NotImplementedException();
        }

        storage.ColliderDataBuffer.Clear();
    }

}
