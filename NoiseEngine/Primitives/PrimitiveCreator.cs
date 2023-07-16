using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;

namespace NoiseEngine.Primitives;

public class PrimitiveCreator {

    private readonly ApplicationScene scene;

    public Shader DefaultShader => Shared.DefaultShader;
    public Material DefaultMaterial => Shared.DefaultMaterial;

    public Mesh CubeMesh => Shared.CubeMesh;

    private PrimitiveCreatorShared Shared { get; }

    internal PrimitiveCreator(ApplicationScene scene) {
        this.scene = scene;
        Shared = PrimitiveCreatorShared.CreateOrGet(scene.GraphicsDevice);
    }

    /// <summary>
    /// Gets or creates sphere mesh with given <paramref name="resolution"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="resolution">Resolution of sphere mesh. Default 3.</param>
    /// <param name="type">
    /// <see cref="SphereType"/> of returned mesh. Default <see cref="SphereType.Icosphere"/>.
    /// </param>
    /// <returns>
    /// Sphere <see cref="Mesh"/> with given <paramref name="resolution"/> and <paramref name="type"/>.
    /// </returns>
    public Mesh GetSphereMesh(uint resolution = 3, SphereType type = SphereType.Icosphere) {
        return Shared.GetSphereMesh(resolution, type);
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
        return scene.Spawn(
            new TransformComponent(
                position ?? Vector3<float>.Zero, rotation ?? Quaternion<float>.Identity, scale ?? Vector3<float>.One
            ),
            new MeshRendererComponent(CubeMesh, DefaultMaterial)
        );
    }

    /// <summary>
    /// Creates primitive sphere.
    /// </summary>
    /// <param name="position">Position of the sphere.</param>
    /// <param name="rotation">Rotation of the sphere.</param>
    /// <param name="scale">Scale of the sphere.</param>
    /// <returns>Sphere <see cref="Entity"/>.</returns>
    public Entity CreateSphere(
        Vector3<float>? position = null, Quaternion<float>? rotation = null, Vector3<float>? scale = null
    ) {
        return scene.Spawn(
            new TransformComponent(
                position ?? Vector3<float>.Zero, rotation ?? Quaternion<float>.Identity, scale ?? Vector3<float>.One
            ),
            new MeshRendererComponent(GetSphereMesh(), DefaultMaterial)
        );
    }

}
