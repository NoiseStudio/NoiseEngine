using NoiseEngine.Mathematics;
using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Nesl.Functions;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VertexPosition3Uv2(Vector3<float> Position, Vector2<float> Uv);
