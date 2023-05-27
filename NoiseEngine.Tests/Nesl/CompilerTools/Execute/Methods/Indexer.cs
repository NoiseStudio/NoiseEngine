using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Linq;
using System;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Execute.Methods;

public class Indexer : NeslTestEnvironment {

    public Indexer(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void LoadAndSet() {
        NeslAssembly assembly = NeslCompiler.Compile(nameof(LoadAndSet), "", new NeslFile[] { new NeslFile("H", @"
            using System;

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = buffer[2];
            }
        ")});

        NeslType type = assembly.Types.Single();
        NeslField? field = type.GetField("buffer");
        Assert.NotNull(field);
        NeslMethod? method = type.GetMethod("Main");
        Assert.NotNull(method);

        Span<float> data = stackalloc float[] {
            -1, 0, 26.05f
        };

        foreach (GraphicsDevice graphicsDevice in GraphicsDevices) {
            GraphicsHostBuffer<float> buffer = new GraphicsHostBuffer<float>(
                graphicsDevice, GraphicsBufferUsage.TransferAll, (ulong)data.Length
            );
            buffer.SetData(data);

            ComputeMaterial material = new ComputeMaterial(new ComputeShader(graphicsDevice, type));
            material.GetProperty(field!)!.SetBuffer(buffer);

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(graphicsDevice, false);
            commandBuffer.Dispatch(material.GetKernel(method!)!, material, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            buffer.GetData(data);
            Assert.Equal(26.05f, data[0]);
        }
    }

}
