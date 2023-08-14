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
        RigidBodyFinalDataComponent data, ref TransformComponent transform, ColliderComponent collider,
        ref RigidBodySleepComponent sleep
    ) {
        if (rigidBody.IsSleeping) {
            if (sleep.WakeUp) {
                rigidBody.SleepAccumulator--;
                sleep = new RigidBodySleepComponent(false);
            }

            ImmovableColliderRegisterSystem.RegisterImmovable(space, entity, transform, collider, true);
            return;
        }

        if (rigidBody.UseGravity) {
            rigidBody.LinearVelocity = rigidBody.LinearVelocity = rigidBody.LinearVelocity with {
                Y = rigidBody.LinearVelocity.Y + gravityAcceleration
            };
        }

        middle.Position = data.TargetPosition + rigidBody.LinearVelocity * fixedDeltaTime;
        data.TargetRotation *= Quaternion.EulerRadians(rigidBody.AngularVelocity * fixedDeltaTime);

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            data.TargetPosition, data.TargetPosition + rigidBody.CenterOfMass, transform.Scale,
            rigidBody.LinearVelocity, rigidBody.InverseInertiaTensorMatrix, rigidBody.InverseMass,
            true
        ), collider));
    }

}
