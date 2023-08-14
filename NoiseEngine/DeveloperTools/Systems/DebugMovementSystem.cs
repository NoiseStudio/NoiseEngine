using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.Inputs;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics;
using System;

namespace NoiseEngine.DeveloperTools.Systems;

public partial class DebugMovementSystem : EntitySystem {

    private const float MouseDownLookLimiter = (float)Math.PI / 2;

    private float speed = 4.5f;
    private float currentSpeed = 4.5f;

    public Window Window { get; }

    public float SpeedIncrease { get; set; } = 0.2f;
    public float SpeedMultipler { get; set; } = 1f;
    public float MinSpeedMultipler { get; set; } = 0.001f;
    public float MaxSpeedMultipler { get; set; } = 2f;
    public float Sensitivity { get; set; } = 0.001f;

    public float Speed {
        get => speed;
        set {
            speed = value;
            currentSpeed = Math.Max(value, currentSpeed);
        }
    }

    public DebugMovementSystem(Window window) {
        Window = window;
        Window.Input.CursorLockMode = CursorLockMode.Locked;
    }

    /// <summary>
    /// Initializes new <see cref="DebugMovementSystem"/> to <paramref name="camera"/>.
    /// </summary>
    /// <param name="camera"><see cref="Camera"/> to which the system will be added.</param>
    /// <returns>New <see cref="DebugMovementSystem"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The <paramref name="camera"/>'s render target must be <see cref="Window"/>.
    /// </exception>
    public static DebugMovementSystem InitializeTo(Camera camera) {
        if (camera.RenderTarget is not Window window)
            throw new InvalidOperationException($"Camera's render target must be {nameof(Window)}.");

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(camera.Entity).Insert(new DebugMovementComponent());
        camera.Scene.ExecuteCommands(commands);

        DebugMovementSystem system = new DebugMovementSystem(window);
        camera.Scene.AddFrameDependentSystem(system);
        return system;
    }

    private void OnUpdateEntity(ref TransformComponent transform, ref DebugMovementComponent movement) {
        WindowInput input = Window.Input;

        Vector3<float> position = transform.Position;
        bool changePosition = false;

        if (input.ScrollDelta.Y != 0) {
            SpeedMultipler = Math.Clamp(
                SpeedMultipler * (input.ScrollDelta.Y > 0 ? 1.1f : 0.9f),
                MinSpeedMultipler,
                MaxSpeedMultipler
            );
        }

        // Move
        if (input.Pressed(Key.W)) {
            position += transform.Front * (currentSpeed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }
        if (input.Pressed(Key.S)) {
            position += transform.Back * (currentSpeed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }

        if (input.Pressed(Key.A)) {
            position += transform.Left * (currentSpeed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }
        if (input.Pressed(Key.D)) {
            position += transform.Right * (currentSpeed * SpeedMultipler * DeltaTimeF);
            changePosition = true;
        }

        // Rotation
        movement = movement with {
            MouseRotation = new Vector2<float>(
                Math.Clamp(
                    (float)input.CursorPositionDelta.Y * Sensitivity + movement.MouseRotation.X,
                    -MouseDownLookLimiter,
                    MouseDownLookLimiter
                ),
                (float)input.CursorPositionDelta.X * Sensitivity + movement.MouseRotation.Y
            )
        };
        Quaternion<float> rotation = Quaternion.EulerRadians(new Vector3<float>(
            movement.MouseRotation.X,
            movement.MouseRotation.Y,
            0
        ));

        if (changePosition) {
            movement = movement with { TimeUntilLastChangedPosition = 0 };
            currentSpeed += currentSpeed * DeltaTimeF * SpeedIncrease;
        } else {
            movement = movement with {
                TimeUntilLastChangedPosition = movement.TimeUntilLastChangedPosition + DeltaTimeF
            };

            if (movement.TimeUntilLastChangedPosition > 0.2f)
                currentSpeed = Speed;
        }

        transform = transform with { Position = position, Rotation = rotation };
    }

}
