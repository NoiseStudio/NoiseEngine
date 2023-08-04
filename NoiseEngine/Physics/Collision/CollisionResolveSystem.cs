using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionResolveSystem : EntitySystem {

    private readonly ContactPointsBuffer contactPoints;

    public CollisionResolveSystem(ContactPointsBuffer contactPoints) {
        this.contactPoints = contactPoints;
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyFinalDataComponent data,
        RigidBodyMiddleDataComponent middle
    ) {
        ContactPointsBufferIterator iterator2 = contactPoints.IterateThrough(entity);
        if (iterator2.MoveNext()) {
            for (int i = 0; i < 1; i++) {
                bool yBlocked = false;
                ContactPointsBufferIterator iterator = contactPoints.IterateThrough(entity);
                while (iterator.MoveNext()) {
                    if (iterator.Current.Normal.Y < 0)
                        yBlocked = true;
                }

                iterator = contactPoints.IterateThrough(entity);
                while (iterator.MoveNext()) {
                    if (iterator.Current.Normal.Y > 0 && yBlocked)
                        continue;

                    Vector3<float> normal = iterator.Current.Normal;
                    middle.Position -= normal * iterator.Current.Depth;

                    if (iterator.Current.Depth > 0.5f) {
                        normal = Quaternion.EulerRadians(
                            Random.Shared.NextSingle() / 10000f,
                            Random.Shared.NextSingle() / 10000f,
                            Random.Shared.NextSingle() / 10000f
                        ) * normal;
                        normal = normal.Normalize();
                    }

                    Vector3<float> relativeVelocity = rigidBody.LinearVelocity - iterator.Current.OtherVelocity;
                    float j = iterator.Current.MinRestitutionPlusOneNegative * relativeVelocity.Dot(normal);
                    j /= iterator.Current.SumInverseMass;
                    j *= rigidBody.InverseMass;
                    rigidBody.LinearVelocity += normal * j;
                }
            }
        }

        if (data.TargetPosition.DistanceSquared(middle.Position) <= 0.005f * 0.005f) {
            if (rigidBody.Sleeped < 2)
                rigidBody.Sleeped++;
        } else {
            rigidBody.Sleeped = 0;
        }

        data.TargetPosition = middle.Position;
        data.MaxDistance = 1 / DeltaTimeF;
    }

}
