using NoiseEngine.Components;
using NoiseEngine.Jobs;

namespace NoiseEngine.Systems;

internal class MeshRendererSystem : EntitySystem<TransformComponent, MeshRendererComponent> {

    public Camera Camera { get; }

    internal MeshRendererResources Resources { get; set; } = null!;

    public MeshRendererSystem(Camera camera) {
        Camera = camera;
    }

    protected override void OnUpdateEntity(
        Entity entity, TransformComponent transform, MeshRendererComponent meshRenderer
    ) {
        Resources.AddMesh(meshRenderer.Mesh, meshRenderer.Material, transform.Matrix);
    }

}
