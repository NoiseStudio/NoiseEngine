using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Rendering.Buffers;

[Collection(nameof(ApplicationCollection))]
public class GraphicsHostBufferTest {

    private const ulong Size = 1024;
    private const int Threads = 64;

    [FactRequire(TestRequirements.Graphics)]
    public void CreateDestroy() {
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices)
            new GraphicsHostBuffer<int>(device, GraphicsBufferUsage.Storage, Size).Dispose();
    }

    [FactRequire(TestRequirements.Graphics)]
    public void SetGetData() {
        int[] data = Enumerable.Range(0, (int)Size).ToArray();

        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            int[] read = new int[Size];

            using GraphicsHostBuffer<int> buffer = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.Storage, Size
            );

            buffer.SetData(data);

            buffer.GetData(read);
            Assert.Equal(data, read);
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void SetGetDataMultithread() {
        Random random = new Random();

        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            Parallel.For(0, Threads, _ => {
                using GraphicsHostBuffer<int> buffer = new GraphicsHostBuffer<int>(device, GraphicsBufferUsage.Storage, Size);

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

}
