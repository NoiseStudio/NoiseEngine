using NoiseEngine.Mathematics;
using System.Runtime.InteropServices;

namespace NoiseEngine.Primitives;

[StructLayout(LayoutKind.Sequential)]
public record struct VertexPosition3Color3(Vector3<float> Position, Vector3<float> Color);
