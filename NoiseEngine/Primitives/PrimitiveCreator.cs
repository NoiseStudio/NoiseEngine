using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Primitives;

public class PrimitiveCreator : IDisposable {

    private readonly ApplicationScene scene;

    public Shader DefaultShader => Shared.DefaultShader;
    public Material DefaultMaterial => Shared.DefaultMaterial;

    private PrimitiveCreatorShared Shared => scene.Application.PrimitiveShared;

    internal PrimitiveCreator(ApplicationScene scene) {
        this.scene = scene;
    }

    /// <summary>
    /// Disposes this <see cref="PrimitiveCreator"/>.
    /// </summary>
    public void Dispose() {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates primitive cube.
    /// </summary>
    /// <param name="position">Position of the cube.</param>
    /// <param name="rotation">Rotation of the cube.</param>
    /// <param name="scale">Scale of the cube.</param>
    /// <returns>Cube <see cref="Entity"/>.</returns>
    public Entity CreateCube(
        Vector3<float>? position = null, Quaternion<float>? rotation = null, Vector3<float>? scale = null
    ) {
        return scene.EntityWorld.NewEntity(
            new TransformComponent(
                position ?? Vector3<float>.Zero, rotation ?? Quaternion<float>.Identity, scale ?? Vector3<float>.One),
            new MeshRendererComponent(Shared.CubeMesh),
            new MaterialComponent(DefaultMaterial)
        );
    }

}
