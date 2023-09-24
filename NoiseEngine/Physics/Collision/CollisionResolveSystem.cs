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
                    middle.Position += (iterator.Current.Normal * iterator.Current.Depth).ToPos();

                if (iterator.Current.Depth >= 0.5f) {
                    middle.Position += new float3(
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f
                    ).ToPos();
                }

                Matrix3x3<float> rotation = Matrix3x3<float>.Rotate(data.TargetRotation);
                Matrix3x3<float> inverseInertia =
                    rotation * rigidBody.InverseInertiaTensorMatrix * rotation.Transpose();

                float3 ra = (iterator.Current.Position - middle.Position).ToFloat() + rigidBody.CenterOfMass;
                float3 rb = (iterator.Current.Position - iterator.Current.OtherPosition).ToFloat();

                float3 rv = iterator.Current.OtherVelocity + iterator.Current.OtherAngularVelocity.Cross(rb) -
                    rigidBody.LinearVelocity - rigidBody.AngularVelocity.Cross(ra);

                float j = rv.Dot(iterator.Current.Normal) * iterator.Current.MinRestitutionPlusOneNegative;
                float sumMass = rigidBody.InverseMass + iterator.Current.InverseMass;
                float denom = iterator.Current.Normal.Dot(
                    (inverseInertia * ra.Cross(iterator.Current.Normal)
                ).Cross(iterator.Current.Normal));

                j /= sumMass + denom;
                float3 impulse = iterator.Current.Normal * j;

                // NOTE: I do not know if normalization is correct, but without it objects rotate too much.
                //       And with it results are similar to physics engines such as PhysX. - Vixen 2023-09-18
                rigidBody.AngularVelocity -= (inverseInertia * ra.Cross(impulse)).Normalize();

                if (notBlocked)
                    rigidBody.LinearVelocity -= impulse * rigidBody.InverseMass;

                // Friction.
                float3 t = -(rv - (iterator.Current.Normal * rv.Dot(iterator.Current.Normal))).Normalize();
                float jt = (-rv.Dot(t)) / (sumMass + denom);

                if (float.Abs(jt) < j * 0.6f)
                    impulse = t * jt;
                else
                    impulse = t * -j * 0.6f;

                rigidBody.AngularVelocity -= (inverseInertia * ra.Cross(impulse)).Normalize();
                if (notBlocked)
                    rigidBody.LinearVelocity -= impulse * rigidBody.InverseMass;
            } while (iterator.MoveNext());

            if (
                rigidBody.LinearVelocity.MagnitudeSquared() <= 0.1f &&
                rigidBody.AngularVelocity.MagnitudeSquared() <= 0.1f
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
