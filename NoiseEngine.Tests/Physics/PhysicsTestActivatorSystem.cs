using NoiseEngine.Components;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics;
using NoiseEngine.Physics.Collision;
using NoiseEngine.Physics.FrameSmoothing;
using NoiseEngine.Physics.Internal;
using NoiseEngine.Physics.Simulation;
using System;

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
        double cycleTime = 20;
        CollisionSpace space = new CollisionSpace();
        ContactPointsBuffer contactPoints = new ContactPointsBuffer();

        SimulationSystem simulationSystem = new SimulationSystem(space);
        CollisionDetectionSystem collisionDetectionSystem = new CollisionDetectionSystem(space, contactPoints);
        CollisionResolveSystem collisionResolveSystem = new CollisionResolveSystem(contactPoints);
        ImmovableColliderRegisterSystem immovableColliderRegisterSystem = new ImmovableColliderRegisterSystem(space);

        simulationSystem.AddDependency(collisionResolveSystem);
        immovableColliderRegisterSystem.AddDependency(collisionDetectionSystem);

        collisionDetectionSystem.AddDependency(simulationSystem);
        collisionDetectionSystem.AddDependency(immovableColliderRegisterSystem);

        collisionResolveSystem.AddDependency(collisionDetectionSystem);

        scene.AddSystem(simulationSystem, cycleTime);
        scene.AddSystem(immovableColliderRegisterSystem, cycleTime);
        scene.AddSystem(collisionDetectionSystem, cycleTime);
        scene.AddSystem(collisionResolveSystem, cycleTime);

        RigidBodyInitializerSystem initalizer = new RigidBodyInitializerSystem();
        scene.AddSystem(initalizer, cycleTime);
    }

    private void OnUpdateEntity() {
    }

}
