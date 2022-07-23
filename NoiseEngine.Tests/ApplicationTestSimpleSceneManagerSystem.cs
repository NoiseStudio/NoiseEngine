using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Presentation;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests;

internal class ApplicationTestSimpleSceneManagerSystem
    : EntitySystem<TransformComponent, ApplicationTestSimpleSceneManagerComponent>
{

    private readonly ApplicationScene scene;

    public override IReadOnlyList<Type> WritableComponents { get; } = new Type[] { typeof(TransformComponent) };

    public ApplicationTestSimpleSceneManagerSystem(ApplicationScene scene) {
        this.scene = scene;
    }

    protected override void OnInitialize() {
        Filter = new EntityFilter(new Type[] { typeof(CameraComponent) });
    }

    protected override void OnUpdateEntity(
        Entity entity, TransformComponent transform, ApplicationTestSimpleSceneManagerComponent value
    ) {
        value = new ApplicationTestSimpleSceneManagerComponent(new Vector3<float>(
            Math.Clamp(value.Rotation.X + 0.4f * DeltaTimeF, 0, float.Pi / 2),
            value.Rotation.Y + 1f * DeltaTimeF,
            0
        ));
        entity.Set(World, value);

        entity.Set(World, transform with {
            Position = transform.Position with { Y = transform.Position.Y + 1f * DeltaTimeF },
            // TODO: implement
            //Rotation = Quaternion.EulerDegress<float>(new Vector3<float>(
            //    value.Rotation.X, value.Rotation.Y, value.Rotation.Z))
        });

        if (Window.TryGetFocusedWindow(out Window? focusedWindow)) {
            Input input = Input.GetInput(focusedWindow);

            if (input.GetKeyDown(Key.Tab, out KeyModifier keyModifier) && keyModifier == KeyModifier.Shift) {
                entity.Add(World, new DebugMovementComponent());

                if (!World.HasAnySystem<DebugMovementSystem>())
                    scene.AddFrameDependentSystem(new DebugMovementSystem());

                Enabled = false;
            }
        }
    }

}
