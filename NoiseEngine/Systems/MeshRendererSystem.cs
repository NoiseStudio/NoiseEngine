using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Rendering;

namespace NoiseEngine.Systems {
    public class MeshRendererSystem : EntitySystem<TransformComponent, MeshRendererComponent, MaterialComponent> {

        public CommandBuffer CommandBuffer { get; }

        public MeshRendererSystem(GraphicsDevice graphicsDevice, Camera camera) {
            CommandBuffer = new CommandBuffer(graphicsDevice, camera);
        }

        protected override void OnUpdate() {
            CommandBuffer.Clear();
        }

        protected override void OnUpdateEntity(
            Entity entity, TransformComponent transform, MeshRendererComponent meshRenderer, MaterialComponent material
        ) {
            CommandBuffer.DrawMesh(meshRenderer.Mesh, material.Material, transform.Matrix);
        }

    }
}
