using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Physics.Collision;
using System.Linq;
using System;

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
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyDataComponent data, TransformComponent transform,
        ColliderComponent collider
    ) {
        rigidBody = rigidBody with { Velocity = rigidBody.Velocity with {
            Y = rigidBody.Velocity.Y - gravityAcceleration
        } };

        data = data with { TargetPosition = data.TargetPosition + rigidBody.Velocity * DeltaTimeF };

        space.RegisterCollider(new ColliderData(entity, new ColliderTransform(
            data.TargetPosition, transform.Rotation, transform.Scale
        ), collider));
    }

}
