using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.IO;
using System.Linq;

namespace NoiseEngine.Tests.Nesl;

public class NeslAssemblyTest : ApplicationTestEnvironment {

    public NeslAssemblyTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void GetTypeString() {
        const string TypeName = "Example";

        NeslAssemblyBuilder assembly = TestEmitHelper.NewAssembly();

        Assert.Null(assembly.GetType(TypeName));
        Assert.Empty(assembly.Types);

        NeslTypeBuilder type = assembly.DefineType(TypeName);

        Assert.Equal(type, assembly.GetType(TypeName));
        Assert.Single(assembly.Types);
    }

    [Fact]
    public void SerializeAndDeserialize() {
        string path = "Fhfgdsfsdf";
        NeslAssembly created = NeslCompiler.Compile("Example", "", new NeslFile[] { new NeslFile(path, @"
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
                    Position = VertexUtils.ObjectToClipPos(data.Position),
                    Color = new f32v4(data.Color, data.Color.X)
                };
            }

            f32v4 Fragment(FragmentData data) {
                return data.Color;
            }
        ") });

        NeslCompilerTest.ExecuteVector3PositionVector3Color(GraphicsDevices, created, path);

        byte[] bytes = created.GetRawBytes();
        NeslAssembly loaded = NeslAssembly.Load(bytes);

        Assert.Equal(created.Name, loaded.Name);
        Assert.True(created.Dependencies.Select(x => x.Name).SequenceEqual(loaded.Dependencies.Select(x => x.Name)));
        Assert.True(
            created.Types.Select(x => x.FullNameWithAssembly)
            .SequenceEqual(loaded.Types.Select(x => x.FullNameWithAssembly))
        );

        NeslCompilerTest.ExecuteVector3PositionVector3Color(GraphicsDevices, loaded, path);
    }

}
