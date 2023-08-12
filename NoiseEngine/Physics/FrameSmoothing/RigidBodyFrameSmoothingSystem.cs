using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Physics.FrameSmoothing;

internal sealed partial class RigidBodyFrameSmoothingSystem : EntitySystem {

    private void OnUpdateEntity(ref TransformComponent transform, ref RigidBodyFinalDataComponent rigidBodyData) {
        float dt = MathF.Min(DeltaTimeF * rigidBodyData.SmoothingMultipler, 1);

        if (transform.Position != rigidBodyData.LastPosition) {
            rigidBodyData.TargetPosition = transform.Position;
            rigidBodyData.LastPosition = transform.Position;
        } else {
            transform = transform with { Position = transform.Position.Lerp(rigidBodyData.TargetPosition, dt) };
            rigidBodyData.LastPosition = transform.Position;
        }

        if (transform.Rotation != rigidBodyData.LastRotation) {
            rigidBodyData.TargetRotation = transform.Rotation;
            rigidBodyData.LastRotation = transform.Rotation;
        } else {
            transform = transform with { Rotation = transform.Rotation.Lerp(rigidBodyData.TargetRotation, dt) };
            rigidBodyData.LastRotation = transform.Rotation;
        }
    }

}
