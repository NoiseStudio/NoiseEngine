using NoiseEngine.Components;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics.FrameSmoothing;
using NoiseEngine.Physics.Systems;

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
        scene.AddSystem(new SimulationSystem(), 150);
    }

    private void OnUpdateEntity(NotUsedComponent notUsed) {
    }

}
