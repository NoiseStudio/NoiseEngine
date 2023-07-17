using NoiseEngine.Jobs;

namespace NoiseEngine.Physics.Simulation;

internal sealed partial class SimulationSystem : EntitySystem {

    private float gravityAcceleration;

    private PhysicsSettings Settings { get; set; } = null!;

    protected override void OnInitialize() {
        Settings = ((ApplicationScene)World).PhysicsSettings;
    }

    protected override void OnUpdate() {
        gravityAcceleration = Settings.GravityF * DeltaTimeF;
    }

    private void OnUpdateEntity(ref RigidBodyComponent rigidBody, ref RigidBodyDataComponent data) {
        rigidBody = rigidBody with { Velocity = rigidBody.Velocity with {
            Y = rigidBody.Velocity.Y - gravityAcceleration
        } };

        data = data with { TargetPosition = data.TargetPosition + rigidBody.Velocity * DeltaTimeF };
    }

}
