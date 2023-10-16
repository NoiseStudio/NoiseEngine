using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Collections.Concurrent;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionResolveSystem : EntitySystem {

    private readonly ConcurrentBag<Entity> debugLines = new ConcurrentBag<Entity>();
    private readonly ContactPointsBuffer contactPoints;
    private float smoothingMultipler;

    public CollisionResolveSystem(ContactPointsBuffer contactPoints) {
        this.contactPoints = contactPoints;
    }

    protected override void OnUpdate() {
        smoothingMultipler = 1000f / (float)(CycleTime ?? throw new InvalidOperationException());

        foreach (Entity entity in debugLines)
            entity.Despawn();
        debugLines.Clear();
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyFinalDataComponent data,
        RigidBodyMiddleDataComponent middle
    ) {
        if (rigidBody.IsSleeping)
            return;

        var angular = rigidBody.AngularVelocity;
        var linear = rigidBody.LinearVelocity;
        var position = middle.Position;

        if (contactPoints.TryIterateThrough(entity, out ContactPointsBufferIterator iterator)) {
            bool yNotBlocked = true;

            ContactPointsBufferIterator iteratorCopy = iterator;
            do {
                if (iteratorCopy.CurrentPoint.Normal.Y > 0)
                    yNotBlocked = false;
            } while (iteratorCopy.MoveNext());

            do {
                float a = iterator.Current.Manifold.Count < 4 ? 1f : 1f;
                //float a = 1;  
                
                bool notBlocked = yNotBlocked || iterator.CurrentPoint.Normal.Y >= 0;
                //if (notBlocked)
                //if (iterator.Current.Manifold.Count >= 4)
                //    position -= (iterator.CurrentPoint.Normal * (iterator.CurrentPoint.Depth / iterator.Current.Manifold.Count)).ToPos();

                if (iterator.CurrentPoint.Depth >= 0.5f) {
                    position += new float3(
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f,
                        (Random.Shared.NextSingle() - 0.5f) / 10000f
                    ).ToPos();
                }

                Matrix3x3<float> rotation = Matrix3x3<float>.Rotate(data.TargetRotation);
                Matrix3x3<float> inverseInertia = 
                    rotation * rigidBody.InverseInertiaTensorMatrix * rotation.Transpose();

                var pos = (data.TargetRotation.ToPos() * iterator.CurrentPoint.Position) + middle.Position;
                SpawnDebug(World, pos, -iterator.CurrentPoint.Normal);

                float3 ra = iterator.CurrentPoint.Position.ToFloat() + rigidBody.CenterOfMass;
                float3 rb = (pos - iterator.Current.OtherPosition).ToFloat();

                float3 rv = iterator.Current.OtherVelocity + iterator.Current.OtherAngularVelocity.Cross(rb) -
                    rigidBody.LinearVelocity - rigidBody.AngularVelocity.Cross(ra);
                float rvDot = iterator.CurrentPoint.Normal.Dot(rv);

                //Log.Info($"{iterator.Current.Manifold.Count} {rvDot}");
                if (rvDot > 0)
                    continue;

                float j = rvDot * iterator.Current.MinRestitutionPlusOneNegative * a;
                //float denom = rigidBody.InverseMass + iterator.Current.OtherInverseMass;
                /* +
                    iterator.CurrentPoint.Normal.Dot(
                        (inverseInertia * ra.Cross(iterator.CurrentPoint.Normal)).Cross(ra) +
                        iterator.CurrentPoint.ResolveImpulseB
                    );*/

                float3 raCrossN = ra.Cross(iterator.CurrentPoint.Normal);

                float d1 = rigidBody.InverseMass + iterator.Current.OtherInverseMass;
                float3 d2 = (inverseInertia * raCrossN).Cross(ra);
                float3 d4 = inverseInertia * raCrossN.Scale(raCrossN);
                float3 d3 = iterator.CurrentPoint.ResolveImpulseB;
                float denom = d1 + iterator.CurrentPoint.Normal.Dot(d2 + d3);

                j /= denom * iterator.Current.Manifold.Count;
                float3 impulse = iterator.CurrentPoint.Normal * j;

                // Friction.
                /*float3 t = -(rv - (iterator.CurrentPoint.Normal * rv.Dot(iterator.CurrentPoint.Normal))).Normalize();
                float jt = (-rv.Dot(t)) / denom;

                if (float.Abs(jt) < j * 0.6f)
                    impulse += t * jt;
                else
                    impulse += t * (-j * 0.6f);*/

                // Add impulse.

                // NOTE: I do not know if normalization is correct, but without it objects rotate too much.
                //       And with it results are similar to physics engines such as PhysX. - Vixen 2023-09-18
                var c = ra.Cross(impulse);
                rigidBody.AngularVelocity -= inverseInertia * c;

                //if (notBlocked)
                rigidBody.LinearVelocity -= impulse * rigidBody.InverseMass;
                //Log.Info($"{linear}");
            } while (iterator.MoveNext());

            //rigidBody.AngularVelocity = angular;
            //rigidBody.LinearVelocity = linear;
            //middle.Position = position;

            /*float b = rigidBody.LinearVelocity.MagnitudeSquared();
            if (b < 0.7f)
                rigidBody.LinearVelocity *= b;

            b = rigidBody.AngularVelocity.MagnitudeSquared();
            if (b < 0.7f)
                rigidBody.AngularVelocity *= b;

            if (
                rigidBody.LinearVelocity.MagnitudeSquared() <= 0.1f &&
                rigidBody.AngularVelocity.MagnitudeSquared() <= 0.1f
            ) {
                //rigidBody.LinearVelocity = float3.Zero;
                //rigidBody.AngularVelocity = float3.Zero;

                rigidBody.SleepAccumulator++;
                if (rigidBody.SleepAccumulator > RigidBodyComponent.SleepThreshold)
                    rigidBody.SleepAccumulator = RigidBodyComponent.MaxSleepAccumulator;

                data.TargetPosition = middle.Position;
                data.SmoothingMultipler = smoothingMultipler;
                return;
            }*/
        }

        rigidBody.SleepAccumulator = Math.Max(rigidBody.SleepAccumulator - 1, 0);

        data.TargetPosition = middle.Position;
        data.SmoothingMultipler = smoothingMultipler;
    }

    private void SpawnDebug(EntityWorld world, pos3 point, float3 normal) {
        Quaternion<float> rotation = Quaternion.LookRotation(normal);
        debugLines.Add(((ApplicationScene)world).Primitive.CreateCube(
            point + (rotation * new float3(0.005f, 0.005f, 2.5f)).ToPos(), rotation, new float3(0.01f, 0.01f, 5f)
        ));
    }

}
