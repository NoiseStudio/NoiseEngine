using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class SolverSystem : EntitySystem {

    private readonly List<Entity> debugLines = new List<Entity>();
    private readonly ContactDataBuffer contactDataBuffer;
    private readonly CollisionSpace space;
    private float invertedCycleTime;
    private int a;

    public SolverSystem(ContactDataBuffer contactDataBuffer, CollisionSpace space) {
        this.contactDataBuffer = contactDataBuffer;
        this.space = space;
    }

    protected override void OnStart() {
        invertedCycleTime = (float)(1000 / (CycleTime ?? throw new InvalidOperationException(
            $"{nameof(CycleTime)} is null."
        )));
    }

    protected override void OnUpdate() {
        a = 0;

        PreStep();
        for (int i = 0; i < 16; i++)
            Step();

        contactDataBuffer.NextFrame();
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyFinalDataComponent data,
        RigidBodyMiddleDataComponent middle
    ) {
        if (space.Transforms.TryGetValue(entity, out ColliderTransform transform)) {
            rigidBody.LinearVelocity = transform.LinearVelocity;
            rigidBody.AngularVelocity = transform.AngularVelocity;
        }

        data.TargetPosition = middle.Position;
        data.SmoothingMultipler = invertedCycleTime;
    }

    private void PreStep() {
        const float AllowedPenetration = 0.01f;
        const float BiasFactor = 0.2f;

        foreach (((Entity bodyA, Entity bodyB, int convexHullId), ContactManifold m) in contactDataBuffer.Data) {
            if (m.FrameId != contactDataBuffer.frameId)
                continue;

            ColliderTransform transformA = space.GetTransform(bodyA);
            ColliderTransform transformB = space.GetTransform(bodyB);
            ContactManifold manifold = m;

            for (byte i = 0; i < manifold.Count; i++) {
                ref ContactPoint point = ref ContactManifold.GetPointRef(ref manifold, i);

                //float3 ra = point.Position.ToFloat();
                //float3 rb = point.PositionB;

                pos3 localPosition = point.Position;
                pos3 worldPosition = transformA.LocalToWorldPosition(localPosition.ToFloat());

                pos3 worldPositionOnB = transformB.LocalToWorldPosition(point.PositionB);
                //point.Depth = (worldPosition - worldPositionOnB).ToFloat().Dot(point.Normal);
                /*point.Depth = (worldPosition - worldPositionOnB).ToFloat().Magnitude();
                if (!point.IsValid) {
                    manifold.RemoveContactPoint(i);
                    continue;
                }*/

                //pos3 worldPosition = localPosition;
                SpawnDebug(World, worldPosition, -point.Normal);

                float3 ra = point.Position.ToFloat();
                float3 rb = point.PositionB.ToFloat();

                float3 raCrossN = ra.Cross(point.Normal);
                float3 rbCrossN = rb.Cross(point.Normal);

                float denom = transformA.InverseMass + transformB.InverseMass;
                denom += raCrossN.Dot(LocalInertiaToWorld(transformA.Rotation, transformA.InverseInertiaTensorMatrix) * raCrossN);
                denom += rbCrossN.Dot(LocalInertiaToWorld(transformB.Rotation, transformB.InverseInertiaTensorMatrix) * rbCrossN);
                point.MassNormal = 1 / denom;

                point.Bias = -BiasFactor * invertedCycleTime * Math.Min(0, -point.Depth + AllowedPenetration);
            }

            contactDataBuffer.UpdateContactPoint(bodyA, bodyB, convexHullId, manifold);
        }
    }

    private void Step() {
        foreach (((Entity bodyA, Entity bodyB, int _), ContactManifold m) in contactDataBuffer.Data) {
            if (m.FrameId != contactDataBuffer.frameId)
                continue;

            ColliderTransform transformA = space.GetTransform(bodyA);
            ColliderTransform transformB = space.GetTransform(bodyB);
            ContactManifold manifold = m;

            for (byte i = 0; i < manifold.Count; i++) {
                ref ContactPoint point = ref ContactManifold.GetPointRef(ref manifold, i);
                if (point.Depth == 0)
                    continue;

                //float3 ra = point.Position.ToFloat();
                //float3 rb = point.PositionB;

                pos3 localPosition = point.Position;
                pos3 worldPosition = (transformA.Rotation.ToPos() * localPosition) + transformA.Position;
                //pos3 worldPosition = localPosition;

                float3 ra = point.Position.ToFloat();
                float3 rb = point.PositionB.ToFloat();

                float3 rv = transformB.LinearVelocity + transformB.AngularVelocity.Cross(rb) -
                        transformA.LinearVelocity - transformA.AngularVelocity.Cross(ra);
                float rvDot = point.Normal.Dot(rv);

                float j = (-rvDot + point.Bias) * point.MassNormal;

                var oldLambda = point.PreviousNormalImpulse;
                point.PreviousNormalImpulse = Math.Max(oldLambda + j, 0);
                j = point.PreviousNormalImpulse - oldLambda;

                float3 impulse = point.Normal * j;

                transformA.AngularVelocity -= LocalInertiaToWorld(transformA.Rotation, transformA.InverseInertiaTensorMatrix) * ra.Cross(impulse);
                transformB.AngularVelocity += LocalInertiaToWorld(transformB.Rotation, transformB.InverseInertiaTensorMatrix) * rb.Cross(impulse);

                transformA.LinearVelocity -= impulse * transformA.InverseMass;
                transformB.LinearVelocity += impulse * transformB.InverseMass;
            }

            if (transformA.IsMovable)
                space.UpdateTransform(bodyA, transformA);
            if (transformB.IsMovable)
                space.UpdateTransform(bodyB, transformB);
        }
    }

    private Matrix3x3<float> LocalInertiaToWorld(
        Quaternion<float> rotation, Matrix3x3<float> inverseInertiaTensorMatrix
    ) {
        Matrix3x3<float> r = Matrix3x3<float>.Rotate(rotation);
        return r * inverseInertiaTensorMatrix * r.Transpose();
    }

    private void SpawnDebug(EntityWorld world, pos3 point, float3 normal) {
        Quaternion<float> rotation = Quaternion.LookRotation(normal);
        pos3 pos = point + (rotation * new float3(0.005f, 0.005f, 2.5f)).ToPos();

        if (a < debugLines.Count) {
            SystemCommands commands = new SystemCommands();
            debugLines[a].TryGet(out TransformComponent c);
            commands.GetEntity(debugLines[a++]).Insert(c with { Position = pos, Rotation = rotation });
            World.ExecuteCommandsAsync(commands);
        } else {
            debugLines.Add(((ApplicationScene)world).Primitive.CreateCube(
                pos, rotation, new float3(0.01f, 0.01f, 5f)
            ));
        }
    }

}
