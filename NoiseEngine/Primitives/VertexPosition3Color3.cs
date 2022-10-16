using NoiseEngine.Mathematics;
using System.Runtime.InteropServices;

namespace NoiseEngine.Primitives;

[StructLayout(LayoutKind.Sequential)]
public record struct VertexPosition3Color3(Vector3<float> Position, Vector3<float> Color) {

    /*internal static VertexDescription GetVertexDescription() {
        return new VertexDescriptionBuilder()
            .AddAttribute(new VertexAttributeDescription(0, 0, VertexAttributeFormat.R32G32B32Float, 0))
            .AddAttribute(
                new VertexAttributeDescription(0, 1, VertexAttributeFormat.R32G32B32Float, sizeof(float) * 3))
            .WithStride(2 * 3 * sizeof(float))
            .Build();
    }*/

}
