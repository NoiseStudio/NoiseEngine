using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Tests;

internal partial class ApplicationTestSimpleSceneManagerSystem : EntitySystem {

    private readonly ApplicationScene scene;
    private readonly Window window;

    public ApplicationTestSimpleSceneManagerSystem(ApplicationScene scene, Window window) {
        this.scene = scene;
        this.window = window;
    }

    protected override void OnInitialize() {
        Filter = new EntityFilter(new Type[] { typeof(CameraComponent) });
    }

    private void OnUpdateEntity(
        Entity entity, SystemCommands commands, ref TransformComponent transform,
        ref ApplicationTestSimpleSceneManagerComponent value
    ) {
        value = new ApplicationTestSimpleSceneManagerComponent(new Vector3<float>(
            Math.Clamp(value.Rotation.X + 0.4f * DeltaTimeF, 0, float.Pi / 2),
            value.Rotation.Y + 1f * DeltaTimeF,
            0
        ));

        transform = transform with {
            Position = transform.Position with { Y = transform.Position.Y + 4f * DeltaTimeF },
            Rotation = Quaternion.EulerRadians(new Vector3<float>(
                value.Rotation.X, value.Rotation.Y, value.Rotation.Z
            ))
        };

        if (window.Input.Pressed(Key.Tab, out KeyModifier keyModifier) && keyModifier.HasFlag(KeyModifier.LeftShift)) {
            commands.GetEntity(entity).Insert(new DebugMovementComponent());
            if (!World.HasAnySystem<DebugMovementSystem>())
                scene.AddFrameDependentSystem(new DebugMovementSystem(window));

            Enabled = false;
        }
    }

}
