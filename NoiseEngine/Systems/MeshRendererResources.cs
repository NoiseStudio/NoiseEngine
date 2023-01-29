using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Systems;

internal class MeshRendererResources {

    private readonly ConcurrentDictionary<
        Shader, ConcurrentDictionary<Material, ConcurrentBag<(Mesh, Matrix4x4<float>)>>
    > meshes = new ConcurrentDictionary<
        Shader, ConcurrentDictionary<Material, ConcurrentBag<(Mesh, Matrix4x4<float>)>>
    >();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMesh(Mesh mesh, Material material, Matrix4x4<float> transform) {
        meshes.GetOrAdd(material.Shader, static _ =>
            new ConcurrentDictionary<Material, ConcurrentBag<(Mesh, Matrix4x4<float>)>>()
        ).GetOrAdd(material, static _ => new ConcurrentBag<(Mesh, Matrix4x4<float>)>()).Add((mesh, transform));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RecordMeshes(GraphicsCommandBuffer commandBuffer) {
        foreach (
            (Shader shader, ConcurrentDictionary<Material, ConcurrentBag<(Mesh, Matrix4x4<float>)>> materialDictionary)
            in meshes.OrderBy(x => x.Key.Priority)
        ) {
            foreach ((Material material, ConcurrentBag<(Mesh, Matrix4x4<float>)> bag) in materialDictionary) {
                foreach ((Mesh, Matrix4x4<float>) meshData in bag)
                    commandBuffer.DrawMeshUnchecked(meshData.Item1, material, meshData.Item2);
            }
        }
    }

    public void Clear() {
        meshes.Clear();
    }

}
