using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionResolveSystem : EntitySystem {

    private readonly ContactPointsBuffer contactPoints;
    private float smoothingMultipler;

    public CollisionResolveSystem(ContactPointsBuffer contactPoints) {
        this.contactPoints = contactPoints;
    }

    protected override void OnUpdate() {
        smoothingMultipler = 1000f / (float)(CycleTime ?? throw new InvalidOperationException());
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyFinalDataComponent data,
        RigidBodyMiddleDataComponent middle
    ) {
        if (contactPoints.TryIterateThrough(entity, out ContactPointsBufferIterator iterator)) {
            bool yNotBlocked = true;

            ContactPointsBufferIterator iteratorCopy = iterator;
            do {
                if (iteratorCopy.Current.Normal.Y < 0)
                    yNotBlocked = false;
            } while (iteratorCopy.MoveNext());

            do {
                bool notBlocked = iterator.Current.Normal.Y <= 0 || yNotBlocked;

                if (notBlocked)
                    middle.Position -= iterator.Current.Normal * iterator.Current.Depth;
                if (iterator.Current.Depth >= 0.5f) {
                    middle.Position += new Vector3<float>(
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f
                    );
                }

                Vector3<float> ra = iterator.Current.Position - middle.Position + rigidBody.CenterOfMass;
                Vector3<float> angularVelocityChange =
                    rigidBody.InverseInertiaTensorMatrix * iterator.Current.Normal.Cross(ra);

                rigidBody.AngularVelocity -= angularVelocityChange;

                if (notBlocked) {
                    float jA = rigidBody.InverseMass + iterator.Current.Normal.Dot(angularVelocityChange.Cross(ra));

                    Vector3<float> relativeVelocity = rigidBody.LinearVelocity - iterator.Current.OtherVelocity;
                    float j =
                        iterator.Current.MinRestitutionPlusOneNegative * relativeVelocity.Dot(iterator.Current.Normal);
                    j /= jA + iterator.Current.ResolveImpulseB;

                    rigidBody.LinearVelocity += iterator.Current.Normal * (j * rigidBody.InverseMass);
                }
            } while (iterator.MoveNext());

            /*if (data.TargetPosition.DistanceSquared(middle.Position) <= 0.01f * 0.01f) {
                data.TargetPosition = middle.Position;
                data.SmoothingMultipler = smoothingMultipler;

                rigidBody.Sleeped += 15;
                if (rigidBody.Sleeped > 20)
                    rigidBody.Sleeped = 20;
                return;
            }*/
        }

        if (rigidBody.Sleeped > 0)
            rigidBody.Sleeped--;

        data.TargetPosition = middle.Position;
        data.SmoothingMultipler = smoothingMultipler;
    }

}
