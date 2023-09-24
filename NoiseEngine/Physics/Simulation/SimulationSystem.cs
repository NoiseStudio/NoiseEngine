using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision;

namespace NoiseEngine.Physics.Simulation;

internal sealed partial class SimulationSystem : EntitySystem {

    private readonly CollisionSpace space;
    private float fixedDeltaTime;
    private float gravityAcceleration;

    private PhysicsSettings Settings { get; set; } = null!;

    public SimulationSystem(CollisionSpace space) {
        this.space = space;
    }

    protected override void OnInitialize() {
        Settings = ((ApplicationScene)World).PhysicsSettings;
    }

    protected override void OnUpdate() {
        fixedDeltaTime = (float)((CycleTime ?? 50) / 1000f);
        gravityAcceleration = Settings.GravityF * fixedDeltaTime;
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyMiddleDataComponent middle,
        ref RigidBodyFinalDataComponent data, ref TransformComponent transform, ColliderComponent collider,
        ref RigidBodySleepComponent sleep
    ) {
        // Sleeping.
        if (rigidBody.IsSleeping) {
            if (sleep.WakeUp) {
                rigidBody.SleepAccumulator--;
                sleep = new RigidBodySleepComponent(false);
            }

            ImmovableColliderRegisterSystem.RegisterImmovable(space, entity, transform, collider, true);
            return;
        }

        // Linear velocity.
        if (rigidBody.UseGravity) {
            rigidBody.LinearVelocity = rigidBody.LinearVelocity = rigidBody.LinearVelocity with {
                Y = rigidBody.LinearVelocity.Y + gravityAcceleration
            };
        }

        rigidBody.LinearVelocity -= rigidBody.LinearVelocity * (rigidBody.LinearDrag * fixedDeltaTime);
        if (rigidBody.LinearVelocity.MagnitudeSquared() < 0.0001f)
            rigidBody.LinearVelocity = float3.Zero;

        middle.Position = data.TargetPosition + (rigidBody.LinearVelocity * fixedDeltaTime).ToPos();

        // Angular velocity.
        rigidBody.AngularVelocity -= rigidBody.AngularVelocity * (rigidBody.AngularDrag * fixedDeltaTime);
        if (rigidBody.AngularVelocity.MagnitudeSquared() < 0.0001f)
            rigidBody.AngularVelocity = float3.Zero;

        Quaternion<float> angularVelocity = new Quaternion<float>(
            rigidBody.AngularVelocity.X,
            rigidBody.AngularVelocity.Y,
            rigidBody.AngularVelocity.Z,
            0
        );
        data.TargetRotation = (
            data.TargetRotation + (angularVelocity * data.TargetRotation * (fixedDeltaTime * 0.5f))
        ).Normalize();

        // Register.
        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            data.TargetPosition, data.TargetRotation, data.TargetPosition + rigidBody.CenterOfMass.ToPos(),
            transform.Scale, rigidBody.LinearVelocity, rigidBody.AngularVelocity, rigidBody.InverseInertiaTensorMatrix,
            rigidBody.InverseMass, true
        ), collider));
    }

}
