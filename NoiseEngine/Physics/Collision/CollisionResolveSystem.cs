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
        if (rigidBody.IsSleeping)
            return;

        if (contactPoints.TryIterateThrough(entity, out ContactPointsBufferIterator iterator)) {
            bool yNotBlocked = true;

            ContactPointsBufferIterator iteratorCopy = iterator;
            do {
                if (iteratorCopy.Current.Normal.Y < 0)
                    yNotBlocked = false;
            } while (iteratorCopy.MoveNext());

            do {
                bool notBlocked = yNotBlocked || iterator.Current.Normal.Y <= 0;
                if (notBlocked)
                    middle.Position -= (iterator.Current.Normal * iterator.Current.Depth).ToPos();

                if (iterator.Current.Depth >= 0.5f) {
                    middle.Position += new float3(
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f
                    ).ToPos();
                }

                float3 ra = (iterator.Current.Position - middle.Position).ToFloat() + rigidBody.CenterOfMass;
                float3 angularVelocityChange =
                    rigidBody.InverseInertiaTensorMatrix * iterator.Current.Normal.Cross(ra);

                rigidBody.AngularVelocity -= angularVelocityChange;

                if (notBlocked) {
                    float jA = rigidBody.InverseMass + iterator.Current.Normal.Dot(angularVelocityChange.Cross(ra));

                    float3 relativeVelocity = rigidBody.LinearVelocity - iterator.Current.OtherVelocity;
                    float j =
                        iterator.Current.MinRestitutionPlusOneNegative * relativeVelocity.Dot(iterator.Current.Normal);
                    j /= jA + iterator.Current.ResolveImpulseB;

                    rigidBody.LinearVelocity += iterator.Current.Normal * (j * rigidBody.InverseMass);
                }
            } while (iterator.MoveNext());

            if (
                rigidBody.LinearVelocity.MagnitudeSquared() <= 0.1f &&
                rigidBody.AngularVelocity.MagnitudeSquared() <= 0.005f * 0.005f
            ) {
                rigidBody.LinearVelocity = float3.Zero;
                rigidBody.AngularVelocity = float3.Zero;

                rigidBody.SleepAccumulator++;
                if (rigidBody.SleepAccumulator > RigidBodyComponent.SleepThreshold)
                    rigidBody.SleepAccumulator = RigidBodyComponent.MaxSleepAccumulator;

                data.TargetPosition = middle.Position;
                data.SmoothingMultipler = smoothingMultipler;
                return;
            }
        }

        rigidBody.SleepAccumulator = Math.Max(rigidBody.SleepAccumulator - 1, 0);

        data.TargetPosition = middle.Position;
        data.SmoothingMultipler = smoothingMultipler;
    }

}
