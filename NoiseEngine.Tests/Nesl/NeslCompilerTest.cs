using NoiseEngine.Nesl;

namespace NoiseEngine.Tests.Nesl;

public class NeslCompilerTest {

    [Fact]
    public void Compile() {
        NeslCompiler.Compile(nameof(Compile), new NeslFile[] { new NeslFile("Path", @"
            using System;

            struct VertexData {
                f32v3 Position;
                f32v3 Color;
            }

            struct FragmentData {
                f32v4 Position;
                f32v4 Color;
            }

            FragmentData Vertex(VertexData data) {
                return new FragmentData() {
                    Position = Vertex.ObjectToClipPos(data.Position),
                    Color = data.Color
                };
            }

            f32v4 Fragment(FragmentData data) {
                return data.Color;
            }
        ") });
    }

}
