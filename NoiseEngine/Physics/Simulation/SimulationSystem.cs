using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Physics.Collision;

namespace NoiseEngine.Physics.Simulation;

internal sealed partial class SimulationSystem : EntitySystem {

    private readonly CollisionSpace space;
    private float gravityAcceleration;

    private PhysicsSettings Settings { get; set; } = null!;

    public SimulationSystem(CollisionSpace space) {
        this.space = space;
    }

    protected override void OnInitialize() {
        Settings = ((ApplicationScene)World).PhysicsSettings;
    }

    protected override void OnUpdate() {
        gravityAcceleration = Settings.GravityF * DeltaTimeF;
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyMiddleDataComponent middle,
        RigidBodyFinalDataComponent data, TransformComponent transform, ColliderComponent collider
    ) {
        if (rigidBody.Sleeped < 2) {
            rigidBody.LinearVelocity = rigidBody.LinearVelocity = rigidBody.LinearVelocity with {
                Y = rigidBody.LinearVelocity.Y + gravityAcceleration
            };

            middle.Position = data.TargetPosition + rigidBody.LinearVelocity * DeltaTimeF;
        }

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            middle.Position, transform.Rotation, transform.Scale, rigidBody.LinearVelocity,
            rigidBody.Sleeped < 2 ? rigidBody.InverseMass : 0, -1.5f
        ), collider));
    }

}
