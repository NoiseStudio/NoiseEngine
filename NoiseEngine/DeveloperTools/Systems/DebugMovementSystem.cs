using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Presentation;
using System;
using System.Collections.Generic;

namespace NoiseEngine.DeveloperTools.Systems;

public class DebugMovementSystem : EntitySystem<TransformComponent, DebugMovementComponent> {

    private const float MouseDownLookLimiter = (float)Math.PI / 2;

    private float speed = 1f;

    public float SpeedIncrease { get; set; } = 0.2f;
    public float SpeedMultipler { get; set; } = 1f;
    public float MinSpeedMultipler { get; set; } = 0.001f;
    public float MaxSpeedMultipler { get; set; } = 2f;
    public float Sensitivity { get; set; } = 0.001f;

    public override IReadOnlyList<Type> WritableComponents { get; } = new Type[] {
        typeof(TransformComponent), typeof(DebugMovementComponent)
    };

    protected override void OnUpdateEntity(
        Entity entity, TransformComponent transform, DebugMovementComponent movement
    ) {
        if (!Window.TryGetFocusedWindow(out Window? window))
            return;

        Input input = Input.GetInput(window);

        Vector3<float> position = transform.Position;
        bool changePosition = false;

        if (input.ScrollOffset.Y != 0) {
            SpeedMultipler = Math.Clamp(
                SpeedMultipler * (input.ScrollOffset.Y > 0 ? 1.1f : 0.9f),
                MinSpeedMultipler,
                MaxSpeedMultipler
            );
        }

        // Move
        if (input.GetKey(Key.W)) {
            position += transform.Front * (speed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }
        if (input.GetKey(Key.S)) {
            position += transform.Back * (speed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }

        if (input.GetKey(Key.A)) {
            position += transform.Left * (speed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }
        if (input.GetKey(Key.D)) {
            position += transform.Right * (speed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }

        // Rotation
        Double2 centerPosition = (Double2)(window.Size / 2);
        Double2 deltaPosition = input.CursorPosition - centerPosition;

        movement = movement with {
            MouseRotation = new Float2(
                Math.Clamp(
                    (float)deltaPosition.Y * Sensitivity + movement.MouseRotation.X,
                    -MouseDownLookLimiter,
                    MouseDownLookLimiter
                ),
                (float)deltaPosition.X * Sensitivity + movement.MouseRotation.Y
            )
        };
        Quaternion<float> rotation = Quaternion.EulerRadians(new Vector3<float>(
            movement.MouseRotation.X,
            movement.MouseRotation.Y,
            0
        ));

        input.CursorPosition = centerPosition;

        if (changePosition) {
            movement = movement with { TimeUntilLastChangedPosition = 0 };
            speed += speed * DeltaTimeF * SpeedIncrease;
        } else {
            movement = movement with {
                TimeUntilLastChangedPosition = movement.TimeUntilLastChangedPosition + DeltaTimeF
            };

            if (movement.TimeUntilLastChangedPosition > 0.2f)
                speed = 1;
        }

        entity.Set(this, transform with { Position = position, Rotation = rotation });
        entity.Set(this, movement);
    }

}
