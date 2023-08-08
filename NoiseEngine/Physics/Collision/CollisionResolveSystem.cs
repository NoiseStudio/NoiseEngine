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

                middle.Position -= iterator.Current.Normal * iterator.Current.Depth;

                if (iterator.Current.Depth >= 0.5f) {
                    middle.Position += new Vector3<float>(
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f
                    );
                }

                Vector3<float> ra = iterator.Current.Position - middle.Position; // Change to center of mass.
                Vector3<float> angularVelocityChange = iterator.Current.Normal.Cross(ra) * 10;
                float jA = rigidBody.InverseMass + iterator.Current.Normal.Dot(angularVelocityChange.Cross(ra));

                Vector3<float> relativeVelocity = rigidBody.LinearVelocity - iterator.Current.OtherVelocity;
                float j =
                    iterator.Current.MinRestitutionPlusOneNegative * relativeVelocity.Dot(iterator.Current.Normal);
                j /= jA + iterator.Current.SumInverseMass;

                rigidBody.LinearVelocity += iterator.Current.Normal * (j * rigidBody.InverseMass);
                rigidBody.AngularVelocity -= angularVelocityChange;
            }

            if (data.TargetPosition.DistanceSquared(middle.Position) <= 0.01f * 0.01f) {
                rigidBody.Sleeped = 20;
                return;
            }
        }

        if (rigidBody.Sleeped > 0)
            rigidBody.Sleeped--;

        data.TargetPosition = middle.Position;
        data.MaxDistance = 1 / DeltaTimeF;
    }

}
