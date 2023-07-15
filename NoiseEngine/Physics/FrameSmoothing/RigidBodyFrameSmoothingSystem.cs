using NoiseEngine.Components;
using NoiseEngine.Jobs;

namespace NoiseEngine.Physics.FrameSmoothing;

internal sealed partial class RigidBodyFrameSmoothingSystem : EntitySystem {

    private void OnUpdateEntity(ref TransformComponent transform, ref RigidBodyDataComponent rigidBodyData) {
        if (transform.Position != rigidBodyData.LastPosition) {
            rigidBodyData = rigidBodyData with {
                TargetPosition = transform.Position,
                LastPosition = transform.Position
            };
        } else {
            transform = transform with { Position = transform.Position.Lerp(rigidBodyData.TargetPosition, DeltaTimeF) };
            rigidBodyData = rigidBodyData with { LastPosition = transform.Position };
        }
    }

}
