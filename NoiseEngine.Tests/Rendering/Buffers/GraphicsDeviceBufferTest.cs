﻿using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Rendering.Buffers;

public class GraphicsDeviceBufferTest : GraphicsTestEnvironment {

    private const ulong Size = 1024;

    private int Threads { get; } = Environment.ProcessorCount * 4;

    public GraphicsDeviceBufferTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics)]
    public void CreateDestroy() {
        foreach (GraphicsDevice device in Fixture.GraphicsDevices)
            _ = new GraphicsDeviceBuffer<int>(device, GraphicsBufferUsage.Storage, Size);
    }

    [FactRequire(TestRequirements.Graphics)]
    public void CreateDestroyMultithread() {
        Random random = new Random();

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            Parallel.For(0, Threads, _ => {
                GraphicsDeviceBuffer<int> buffer = new GraphicsDeviceBuffer<int>(
                    device, GraphicsBufferUsage.TransferAll, Size
                );

                int[] data = new int[Size];
                for (int i = 0; i < (int)Size; i++)
                    data[i] = random.Next();

                buffer.SetData(data);

                int[] read = new int[Size];
                buffer.GetData(read);

                Assert.Equal(data, read);
            });
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void SetGetData() {
        int[] data = Enumerable.Range(0, (int)Size).ToArray();

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            int[] read = new int[Size];

            GraphicsDeviceBuffer<int> buffer = new GraphicsDeviceBuffer<int>(
                device, GraphicsBufferUsage.TransferAll, Size
            );

            buffer.SetData(data);

            buffer.GetData(read);
            Assert.Equal(data, read);
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void SetGetDataMultithread() {
        int[] data = Enumerable.Range(0, (int)Size).ToArray();

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            GraphicsDeviceBuffer<int> buffer = new GraphicsDeviceBuffer<int>(
                device, GraphicsBufferUsage.TransferAll, Size
            );

            Parallel.For(0, Threads, _ => {
                int[] read = new int[Size];
                buffer.SetData(data);

                buffer.GetData(read);
                Assert.Equal(data, read);
            });
        }
    }

}
