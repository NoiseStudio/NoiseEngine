using NoiseEngine.Components;
using NoiseEngine.Jobs;

namespace NoiseEngine.Systems;

internal partial class MeshRendererSystem : EntitySystem {

    public Camera Camera { get; }

    internal MeshRendererResources Resources { get; set; } = null!;

    public MeshRendererSystem(Camera camera) {
        Camera = camera;
    }

    private void OnUpdateEntity(TransformComponent transform, MeshRendererComponent meshRenderer) {
        Resources.AddMesh(meshRenderer.Mesh, meshRenderer.Material, transform.Matrix);
    }

}
