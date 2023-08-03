using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Collections.Generic;

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
                    if (iterator.Current.Normal.Y <= 0 || !yBlocked)
                        middle.Position -= iterator.Current.Normal * iterator.Current.Depth;
                    else
                        continue;

                    Vector3<float> relativeVelocity = rigidBody.Velocity - iterator.Current.OtherVelocity;
                    //Vector3<float> relativeVelocity = rigidBody.Velocity;
                    //if (iterator.Current.OtherIsRigidBody && iterator.Current.OtherEntity.TryGet(out RigidBodyComponent dasd))
                    //    relativeVelocity -= dasd.Velocity;

                    float e = 0.1f;
                    float j = -(1 + e) * relativeVelocity.Dot(iterator.Current.Normal);
                    if (iterator.Current.OtherIsRigidBody)
                        j /= 2;
                    rigidBody.Velocity += iterator.Current.Normal * j;

                    //Log.Debug($"T: {data.TargetPosition} M: {middle.Position} V: {rigidBody.Velocity} J: {j}");
                    //Log.Debug($"{(DeltaTimeF * 1000)} T: {data.TargetPosition} M: {middle.Position} V: {rigidBody.Velocity}");

                    //rigidBody.Velocity = rigidBody.Velocity.Scale(iterator.Current.Normal) * 0.5f;
                    //if (rigidBody.Velocity.MagnitudeSquared() < 1f)
                    //    rigidBody.Velocity = Vector3<float>.Zero;
                    //data.MaxDistance = data.TargetPosition.Distance(data.LastPosition) * (1 / DeltaTimeF);
                    //return;
                }
            }

            //if (MathF.Abs(rigidBody.Velocity.Y) < 0.05f * (DeltaTimeF * 1000))
            //    rigidBody.Velocity = Vector3<float>.Zero;
            //if (data.TargetPosition.Distance(middle.Position) < 0.003f * (DeltaTimeF * 1000))
            //    middle.Position = data.TargetPosition;
        }

        if (data.TargetPosition.Distance(middle.Position) <= 0.005f) {
            if (rigidBody.Sleeped == 0)
                rigidBody.Sleeped = 1;
            else
                rigidBody.Sleeped = 2;
        } else {
            rigidBody.Sleeped = 0;
        }

        data.TargetPosition = middle.Position;
        data.MaxDistance = 1 / DeltaTimeF;
    }

}
