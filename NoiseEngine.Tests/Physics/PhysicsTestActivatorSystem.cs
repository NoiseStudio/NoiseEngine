using NoiseEngine.Components;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision;
using NoiseEngine.Physics.FrameSmoothing;
using NoiseEngine.Physics.Simulation;

namespace NoiseEngine.Tests.Physics;

internal partial class PhysicsTestActivatorSystem : EntitySystem {

    private readonly ApplicationScene scene;
    private readonly Window window;

    public PhysicsTestActivatorSystem(ApplicationScene scene, Window window) {
        this.scene = scene;
        this.window = window;
    }

    protected override void OnUpdate() {
        if (!window.Input.Pressed(Key.F1))
            return;
        Enabled = false;

        scene.Primitive.CreateCube(
            new Vector3<float>(0, 0, 15),
            Quaternion.EulerDegrees(new Vector3<float>(0, 90, 0)),
            new Vector3<float>(10, 10, 10)
        );

        scene.AddFrameDependentSystem(new RigidBodyFrameSmoothingSystem());
        double cycleTime = 10;
        CollisionSpace space = new CollisionSpace();
        ContactPointsBuffer contactPoints = new ContactPointsBuffer();

        SimulationSystem simulationSystem = new SimulationSystem(space);
        ColliderSpaceRegisterSystem colliderSpaceRegisterSystem = new ColliderSpaceRegisterSystem(space);
        CollisionDetectionSystem collisionDetectionSystem = new CollisionDetectionSystem(space, contactPoints);
        CollisionResolveSystem collisionResolveSystem = new CollisionResolveSystem(contactPoints);

        simulationSystem.AddDependency(collisionResolveSystem);
        colliderSpaceRegisterSystem.AddDependency(collisionDetectionSystem);

        collisionDetectionSystem.AddDependency(simulationSystem);
        collisionDetectionSystem.AddDependency(colliderSpaceRegisterSystem);

        collisionResolveSystem.AddDependency(collisionDetectionSystem);

        scene.AddSystem(simulationSystem, cycleTime);
        scene.AddSystem(colliderSpaceRegisterSystem, cycleTime);
        scene.AddSystem(collisionDetectionSystem, cycleTime);
        scene.AddSystem(collisionResolveSystem, cycleTime);
    }

    private void OnUpdateEntity() {
    }

}
