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
        RigidBodyFinalDataComponent data, ref TransformComponent transform, ColliderComponent collider
    ) {
        if (rigidBody.Sleeped < 20) {
            rigidBody.LinearVelocity = rigidBody.LinearVelocity = rigidBody.LinearVelocity with {
                Y = rigidBody.LinearVelocity.Y + gravityAcceleration
            };
            transform = transform with {
                Rotation = transform.Rotation * Quaternion.EulerRadians(rigidBody.AngularVelocity * fixedDeltaTime)
            };

            middle.Position = data.TargetPosition + rigidBody.LinearVelocity * fixedDeltaTime;
        }

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            middle.Position, middle.Position + rigidBody.CenterOfMass, transform.Scale, rigidBody.LinearVelocity,
            rigidBody.InverseInertiaTensorMatrix, rigidBody.Sleeped < 20 ? rigidBody.InverseMass : 0, -1.1f
        ), collider));
    }

}
