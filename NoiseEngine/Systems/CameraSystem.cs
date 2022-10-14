using NoiseEngine.Components;
using NoiseEngine.Jobs;

namespace NoiseEngine.Systems;

public class CameraSystem : EntitySystem<TransformComponent, CameraComponent> {

    protected override void OnUpdateEntity(Entity entity, TransformComponent transform, CameraComponent camera) {
        /*Camera? c = camera.RenderCamera.Camera;
        if (c is null)
            return;

        c.Position = new Float3(transform.Position.X, transform.Position.Y, transform.Position.Z);*/
        // TODO: implement
        //c.Rotation = transform.Rotation;
    }

}
