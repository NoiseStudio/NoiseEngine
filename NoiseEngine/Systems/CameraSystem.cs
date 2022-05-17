using NoiseEngine.Components;
using NoiseEngine.Jobs;

namespace NoiseEngine.Systems {
    public class CameraSystem : EntitySystem<TransformComponent, CameraComponent> {

        protected override void OnUpdateEntity(Entity entity, TransformComponent transform, CameraComponent camera) {
            camera.Camera.Position = transform.Position;
            camera.Camera.Rotation = transform.Rotation;
        }

    }
}
