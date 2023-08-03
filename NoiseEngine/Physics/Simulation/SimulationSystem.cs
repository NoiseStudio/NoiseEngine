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
            rigidBody.Velocity = rigidBody.Velocity = rigidBody.Velocity with {
                Y = rigidBody.Velocity.Y - gravityAcceleration
            };

            middle.Position = data.TargetPosition + rigidBody.Velocity * DeltaTimeF;
        }

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            middle.Position, transform.Rotation, transform.Scale, rigidBody.Velocity, rigidBody.Sleeped < 2
        ), collider));
    }

}
